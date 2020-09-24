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
        private readonly EFCoreShardingOptions _shardingOptions;
        public GenericDbContext(DbContextOptions contextOptions, GenericDbContextOptions options, EFCoreShardingOptions shardingOptions)
            : base(contextOptions)
        {
            Options = options;
            _shardingOptions = shardingOptions;

            Database.SetCommandTimeout(_shardingOptions.CommandTimeout);
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
                var q = _shardingOptions.Types.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!Options.EntityNamespace.IsNullOrEmpty())
                {
                    q = q.Where(x => x.Namespace.Contains(Options.EntityNamespace));
                }

                entityTypes = q.ToList();
            }

            entityTypes.ForEach(aEntity =>
            {
                var entity = modelBuilder.Entity(aEntity);
                _shardingOptions.EntityTypeBuilderFilter?.Invoke(entity);

                if (!string.IsNullOrEmpty(Options.Suffix))
                {
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Options.Suffix}");
                }
            });

            //支持IEntityTypeConfiguration配置
            var entityTypeConfigurationTypes = _shardingOptions.Types
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
