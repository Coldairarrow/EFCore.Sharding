using EFCore.Sharding.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// 通用DbContext
    /// </summary>
    public class GenericDbContext : DbContext
    {
        /// <summary>
        /// 当前DbContext所在注入周期
        /// </summary>
        public IServiceProvider ServiceProvider;

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

        internal readonly string CreateStackTrace;
        internal readonly DateTimeOffset CreateTime;
        internal string FirstCallStackTrace;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextOptions"></param>
        /// <param name="paramter"></param>
        /// <param name="shardingOptions"></param>
        /// <param name="serviceProvider"></param>
        public GenericDbContext(DbContextOptions contextOptions, DbContextParamters paramter, EFCoreShardingOptions shardingOptions, IServiceProvider serviceProvider)
            : base(contextOptions)
        {
            ServiceProvider = serviceProvider;

            CreateTime = DateTimeOffset.Now;
            CreateStackTrace = Environment.StackTrace;
            Cache.DbContexts.Add(this);

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
            : this(dbContext.DbContextOption, dbContext.Paramter, dbContext.ShardingOption, dbContext.ServiceProvider)
        {

        }
        private static readonly ValueConverter<DateTime, DateTime> _dateTimeConverter
            = new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Local));

        /// <summary>
        /// 模型构建
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            FirstCallStackTrace = Environment.StackTrace;

            List<Type> entityTypes;
            if (Paramter.EntityTypes?.Length > 0)
            {
                entityTypes = Paramter.EntityTypes.ToList();
            }
            else
            {
                IEnumerable<Type> q = EFCoreShardingOptions.Types.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!Paramter.EntityNamespace.IsNullOrEmpty())
                {
                    q = q.Where(x => x.Namespace.Contains(Paramter.EntityNamespace));
                }

                entityTypes = q.ToList();
            }

            entityTypes.ForEach(aEntity =>
            {
                Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder entity = modelBuilder.Entity(aEntity);

                ShardingOption.EntityTypeBuilderFilter?.Invoke(entity);

                if (!string.IsNullOrEmpty(Paramter.Suffix))
                {
                    _ = entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{Paramter.Suffix}", AnnotationHelper.GetDbSchemaName(aEntity));
                }
            });

            //支持IEntityTypeConfiguration配置
            entityTypes.ForEach(aEntityType =>
            {
                List<Type> entityTypeConfigurationTypes = EFCoreShardingOptions.Types
                    .Where(x => x.GetInterfaces().Any(y =>
                        y.IsGenericType
                        && y.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        && aEntityType == y.GetGenericArguments()[0])
                        )
                    .ToList();
                entityTypeConfigurationTypes.ForEach(aEntityConfig =>
                {
                    MethodInfo method = modelBuilder.GetType().GetMethods()
                        .Where(x => x.Name == nameof(ModelBuilder.ApplyConfiguration)
                            && x.GetParameters().Count() == 1
                            && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        )
                        .FirstOrDefault();

                    _ = method.MakeGenericMethod(aEntityType).Invoke(modelBuilder, new object[] { Activator.CreateInstance(aEntityConfig) });
                });
            });

            //DateTime默认为Local
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(_dateTimeConverter);
                    }
                }
            }

            //字段注释,需要开启程序集XML文档
            if (ShardingOption.EnableComments)
            {
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
                {
                    foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entityType.GetProperties())
                    {
                        if (property.PropertyInfo == null)
                        {
                            continue;
                        }

                        StringBuilder comment = new(property.PropertyInfo.GetXmlDocsSummary());

                        if (property.PropertyInfo.PropertyType.IsEnum)
                        {
                            foreach (object aValue in Enum.GetValues(property.PropertyInfo.PropertyType))
                            {
                                string memberComment = property.PropertyInfo.PropertyType.GetMembers()
                                    .Where(x => x.Name == aValue.ToString())
                                    .FirstOrDefault()?
                                    .GetXmlDocsSummary();
                                _ = comment.Append($" {(int)aValue}={memberComment}");
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
                {
                    aEntry.State = EntityState.Detached;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            int count = 0;

            if (ShardingOption.OnSaveChanges != null)
            {
                AsyncHelper.RunSync(() => ShardingOption.OnSaveChanges?.Invoke(ServiceProvider, this, async () =>
                {
                    count = await base.SaveChangesAsync();
                }));
            }
            else
            {
                count = base.SaveChanges();
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int count = 0;

            if (ShardingOption.OnSaveChanges != null)
            {
                await ShardingOption.OnSaveChanges?.Invoke(ServiceProvider, this, async () =>
                {
                    count = await base.SaveChangesAsync();
                });
            }
            else
            {
                count = await base.SaveChangesAsync();
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            _ = Cache.DbContexts.Remove(this);

            base.Dispose();
        }
    }
}
