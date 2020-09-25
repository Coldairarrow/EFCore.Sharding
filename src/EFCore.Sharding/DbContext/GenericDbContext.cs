using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EFCore.Sharding
{
    public class GenericDbContext : DbContext
    {
        public DbContextOptions DbContextOption { get; }
        public EFCoreShardingOptions ShardingOption { get; }
        public DbContextParamters Paramter { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextOptions"></param>
        /// <param name="paramter"></param>
        /// <param name="shardingOptions"></param>
        public GenericDbContext(DbContextOptions contextOptions, DbContextParamters paramter, EFCoreShardingOptions shardingOptions)
            : base(contextOptions)
        {
            DbContextOption = contextOptions;
            Paramter = paramter;
            ShardingOption = shardingOptions;

            Database.SetCommandTimeout(ShardingOption.CommandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public GenericDbContext(GenericDbContext dbContext)
            : this(dbContext.DbContextOption, dbContext.Paramter, dbContext.ShardingOption)
        {

        }
        private static readonly ValueConverter<DateTime, DateTime> _dateTimeConverter
            = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Local));
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<Type> entityTypes;
            if (Paramter.EntityTypes?.Length > 0)
            {
                entityTypes = Paramter.EntityTypes.ToList();
            }
            else
            {
                var q = ShardingOption.Types.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!Paramter.EntityNamespace.IsNullOrEmpty())
                {
                    q = q.Where(x => x.Namespace.Contains(Paramter.EntityNamespace));
                }

                entityTypes = q.ToList();
            }

            entityTypes.ForEach(aEntity =>
            {
                var entity = modelBuilder.Entity(aEntity);
                ShardingOption.EntityTypeBuilderFilter?.Invoke(entity);

                if (!string.IsNullOrEmpty(Paramter.Suffix))
                {
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Paramter.Suffix}");
                }
            });

            //支持IEntityTypeConfiguration配置
            var entityTypeConfigurationTypes = ShardingOption.Types
                .Where(x => x.GetInterfaces().Any(y =>
                    y.IsGenericType
                    && y.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                    && entityTypes.Contains(y.GetGenericArguments()[0])
                    ))
                .ToList();
            entityTypeConfigurationTypes.ForEach(aConfig =>
            {
                modelBuilder.ApplyConfiguration((dynamic)Activator.CreateInstance(aConfig));
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
