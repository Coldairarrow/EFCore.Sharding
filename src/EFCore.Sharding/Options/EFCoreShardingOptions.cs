using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// 程序集路径
        /// </summary>
        public readonly List<string> AssemblyPaths = new List<string>();

        /// <summary>
        /// 实体模型构建过滤器
        /// </summary>
        public Action<EntityTypeBuilder> EntityTypeBuilderFilter { get; set; }

        /// <summary>
        /// 使用Code First进行迁移时是否忽略外键（即不生成数据库外键）,默认为false(即默认生成外键)
        /// </summary>
        public bool MigrationsWithoutForeignKey { get; set; } = false;

        private Type[] _types;
        private readonly object _typesLock = new object();
        internal Type[] Types
        {
            get
            {
                if (_types == null)
                {
                    lock (_typesLock)
                    {
                        if (_types == null)
                        {
                            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            AssemblyPaths.Add(rootPath);

                            var assemblies = AssemblyPaths.Distinct()
                                .SelectMany(x => Directory.GetFiles(x, "*.dll"))
                                .Where(x => !new FileInfo(x).Name.StartsWith("System")
                                    && !new FileInfo(x).Name.StartsWith("Microsoft"))
                                .Select(x => Assembly.LoadFrom(x))
                                .Where(x => !x.IsDynamic)
                                .Concat(new Assembly[] { Assembly.GetEntryAssembly() })
                                .Distinct()
                                .ToList();

                            List<Type> types = new List<Type>();
                            assemblies.ForEach(aAssembly =>
                            {
                                try
                                {
                                    types.AddRange(aAssembly.GetTypes());
                                }
                                catch
                                {

                                }
                            });

                            _types = types.ToArray();
                        }
                    }
                }

                return _types;
            }
        }
        internal Action<IServiceProvider> Bootstrapper;
    }
}
