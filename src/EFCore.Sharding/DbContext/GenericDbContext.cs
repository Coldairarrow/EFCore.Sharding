using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EFCore.Sharding
{
    /// <summary>
    /// 通用DbContext
    /// </summary>
    public class GenericDbContext : DbContext
    {
        /// <summary>
        /// DbContext原生配置
        /// </summary>
        public DbContextOptions DbContextOption { get; }

        /// <summary>
        /// 全局自定义配置
        /// </summary>
        public EFCoreShardingOptions ShardingOption { get; }

        /// <summary>
        /// 构建参数
        /// </summary>
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

        /// <summary>
        /// 模型构建
        /// </summary>
        /// <param name="modelBuilder"></param>
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
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Paramter.Suffix}", AnnotationHelper.GetDbSchemaName(aEntity));
                }
            });

            //支持IEntityTypeConfiguration配置
            entityTypes.ForEach(aEntityType =>
            {
                var entityTypeConfigurationTypes = ShardingOption.Types
                    .Where(x => x.GetInterfaces().Any(y =>
                        y.IsGenericType
                        && y.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        && aEntityType == y.GetGenericArguments()[0])
                        )
                    .ToList();
                entityTypeConfigurationTypes.ForEach(aEntityConfig =>
                {
                    var method = modelBuilder.GetType().GetMethods()
                        .Where(x => x.Name == nameof(ModelBuilder.ApplyConfiguration)
                            && x.GetParameters().Count() == 1
                            && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        )
                        .FirstOrDefault();

                    method.MakeGenericMethod(aEntityType).Invoke(modelBuilder, new object[] { Activator.CreateInstance(aEntityConfig) });
                });
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

            //字段注释,需要开启程序集XML文档
            if (ShardingOption.EnableComments)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.PropertyInfo == null)
                        {
                            continue;
                        }

                        StringBuilder comment = new StringBuilder(property.PropertyInfo.GetXmlDocsSummary());

                        if (property.PropertyInfo.PropertyType.IsEnum)
                        {
                            foreach (var aValue in Enum.GetValues(property.PropertyInfo.PropertyType))
                            {
                                var memberComment = property.PropertyInfo.PropertyType.GetMembers()
                                    .Where(x => x.Name == aValue.ToString())
                                    .FirstOrDefault()?
                                    .GetXmlDocsSummary();
                                comment.Append($" {(int)aValue}={memberComment}");
                            }
                        }
                        property.SetComment(comment.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 取消跟踪
        /// </summary>
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
