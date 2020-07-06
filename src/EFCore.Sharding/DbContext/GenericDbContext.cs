using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class GenericDbContext : DbContext
    {
        public GenericDbContext(GenericDbContextOptions options)
            : base(options.ContextOptions)
        {
            Options = options;
        }
        public GenericDbContextOptions Options { get; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<Type> entityTypes;
            if (Options.EntityTypes?.Length > 0)
            {
                entityTypes = Options.EntityTypes.ToList();
            }
            else
            {
                var q = ShardingConfig.AllEntityTypes.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!Options.EntityNamespace.IsNullOrEmpty())
                {
                    q = q.Where(x => x.Namespace.Contains(Options.EntityNamespace));
                }

                entityTypes = q.ToList();
            }

            //支持IEntityTypeConfiguration配置
            entityTypes.Select(x => x.Assembly).Distinct().ToList().ForEach(aAssembly =>
            {
                modelBuilder.ApplyConfigurationsFromAssembly(aAssembly);
            });

            entityTypes.ForEach(aEntity =>
            {
                var entity = modelBuilder.Entity(aEntity);
                if (!string.IsNullOrEmpty(Options.Suffix))
                {
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Options.Suffix}");
                }
            });

        }
        public IQueryable GetIQueryable(Type entityType)
        {
            var dbSet = GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(this, null);
            var resQ = typeof(EntityFrameworkQueryableExtensions).GetMethod("AsNoTracking").MakeGenericMethod(entityType).Invoke(null, new object[] { dbSet });

            return resQ as IQueryable;
        }
        public void Detach()
        {
            ChangeTracker.Entries().ToList().ForEach(aEntry =>
            {
                if (aEntry.State != EntityState.Detached)
                    aEntry.State = EntityState.Detached;
            });
        }
        public override int SaveChanges()
        {
            int count = base.SaveChanges();
            Detach();

            return count;
        }
        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int count = await base.SaveChangesAsync(cancellationToken);
            Detach();

            return count;
        }
    }
}
