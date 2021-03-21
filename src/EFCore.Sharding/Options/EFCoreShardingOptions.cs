using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Reflection;

namespace EFCore.Sharding
{
    /// <summary>
    /// EFCoreSharding配置参数
    /// </summary>
    public class EFCoreShardingOptions
    {
        /// <summary>
        /// SQL执行超时时间,默认30S,单位（秒）
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// 是否使用逻辑删除,默认否
        /// </summary>
        public bool LogicDelete { get; set; } = false;

        /// <summary>
        /// 主键字段名
        /// </summary>
        public string KeyField { get; set; } = "Id";

        /// <summary>
        /// 标记已删除字段
        /// </summary>
        public string DeletedField { get; set; } = "Deleted";

        /// <summary>
        /// 实体程序集
        /// </summary>
        public Assembly[] EntityAssemblies { get; set; } = Array.Empty<Assembly>();

        /// <summary>
        /// 实体模型构建过滤器
        /// </summary>
        public Action<EntityTypeBuilder> EntityTypeBuilderFilter { get; set; }

        /// <summary>
        /// 使用Code First进行迁移时是否忽略外键（即不生成数据库外键）,默认为false(即默认生成外键)
        /// </summary>
        public bool MigrationsWithoutForeignKey { get; set; } = false;

        /// <summary>
        /// 是否在启动时自动创建分表,默认true
        /// </summary>
        public bool CreateShardingTableOnStarting { get; set; } = true;

        /// <summary>
        /// 是否启用分表数据库迁移,默认false
        /// </summary>
        public bool EnableShardingMigration { get; set; } = false;

        /// <summary>
        /// 是否启用注释，默认false，建议在数据库迁移时开启
        /// </summary>
        public bool EnableComments { get; set; } = false;

        private Type[] _types = Array.Empty<Type>();
        internal Type[] Types
        {
            get
            {
                if (_types.Length == 0)
                {
                    if (EntityAssemblies.Length == 0)
                    {
                        throw new Exception("请通过SetEntityAssemblies指定实体程序集");
                    }

                    _types = EntityAssemblies.SelectMany(x => x.GetTypes()).ToArray();
                }

                return _types;
            }
        }
        internal Action<IServiceProvider> Bootstrapper;
    }
}
