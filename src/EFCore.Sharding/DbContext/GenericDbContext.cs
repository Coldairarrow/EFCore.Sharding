using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EFCore.Sharding
{
    internal class GenericDbContext : DbContext
    {
        public GenericDbContext(GenericDbContextOptions options)
            : base(options.ContextOptions)
        {
            Options = options;
            Database.SetCommandTimeout(Constant.CommandTimeout);
        }
        public GenericDbContextOptions Options { get; }
        private static readonly ValueConverter<DateTime, DateTime> _dateTimeConverter
            = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Local));
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<Type> entityTypes;
            if (Options.EntityTypes?.Length > 0)
            {
                entityTypes = Options.EntityTypes.ToList();
            }
            else
            {
                var q = Constant.AllEntityTypes.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!Options.EntityNamespace.IsNullOrEmpty())
                {
                    q = q.Where(x => x.Namespace.Contains(Options.EntityNamespace));
                }

                entityTypes = q.ToList();
            }

            //支持IEntityTypeConfiguration配置
            Constant.Assemblies.Distinct().ToList().ForEach(aAssembly =>
            {
                modelBuilder.ApplyConfigurationsFromAssembly(aAssembly, x =>
                {
                    //仅加载对应实体配置
                    return entityTypes.Any(y => typeof(IEntityTypeConfiguration<>).MakeGenericType(y).IsAssignableFrom(x));
                });
            });

            entityTypes.ForEach(aEntity =>
            {
                var entity = modelBuilder.Entity(aEntity);
                if (!string.IsNullOrEmpty(Options.Suffix))
                {
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Options.Suffix}");
                }
            });

            //DateTime默认为Local
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(_dateTimeConverter);
                }
            }
        }
        public IQueryable GetIQueryable(Type entityType, bool tracking)
        {
            var iq = GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(this, null) as IQueryable;

            if (!tracking)
            {
                iq = typeof(EntityFrameworkQueryableExtensions).GetMethod("AsNoTracking").MakeGenericMethod(entityType).Invoke(null, new object[] { iq }) as IQueryable;
            }

            return iq;
        }
        public void Detach()
        {
            ChangeTracker.Entries().ToList().ForEach(aEntry =>
            {
                if (aEntry.State != EntityState.Detached)
                    aEntry.State = EntityState.Detached;
            });
        }
    }
}
