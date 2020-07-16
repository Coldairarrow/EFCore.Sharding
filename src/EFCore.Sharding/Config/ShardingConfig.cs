using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Sharding
{
    /// <summary>
    /// 配置
    /// </summary>
    public static class ShardingConfig
    {
        #region 外部接口

        /// <summary>
        /// 默认抽象数据库名
        /// </summary>
        public const string DefaultAbsDbName = "BaseDb";

        /// <summary>
        /// 默认数据库组名
        /// </summary>
        public const string DefaultDbGourpName = "BaseDbGroup";

        /// <summary>
        /// 注入EFCoreSharding
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configInit">配置项</param>
        /// <returns></returns>
        public static IServiceCollection AddEFCoreSharding(this IServiceCollection services, Action<IConfigInit> configInit)
        {
            if (_inited)
                throw new Exception("只能初始化一次");
            if (configInit == null)
                throw new Exception("配置不能为null");

            if (ServiceDescriptors == null)
                ServiceDescriptors = services;

            _inited = true;

            MemoryConfigProvider memoryConfigProvider = new MemoryConfigProvider();
            configInit(memoryConfigProvider);

            ConfigProvider = memoryConfigProvider;

            services.AddScoped(_ =>
            {
                return DbFactory.GetShardingDbAccessor();
            });

            return services;
        }

        #endregion

        #region 私有成员

        internal static bool LogicDelete { get; set; } = false;
        internal static string KeyField { get; set; } = "Id";
        internal static string DeletedField { get; set; } = "Deleted";
        internal static IServiceCollection ServiceDescriptors;
        internal static List<string> AssemblyNames = new List<string>();
        internal static List<string> AssemblyPaths = new List<string>() { Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) };
        internal static void CheckInit()
        {
            if (!_inited)
                throw new Exception("未配置相关参数,请使用ShardingConfig.Init初始化");
        }
        private static bool _inited = false;
        internal static IConfigProvider ConfigProvider { get; set; }
        private static List<Type> _allEntityTypes;
        private static object _lock = new object();
        internal static List<Type> AllEntityTypes
        {
            get
            {
                if (_allEntityTypes == null)
                {
                    lock (_lock)
                    {
                        if (_allEntityTypes == null)
                        {
                            _allEntityTypes = new List<Type>();

                            Expression<Func<string, bool>> where = x => true;
                            where = where.And(x =>
                                  !x.Contains("System.")
                                  && !x.Contains("Microsoft."));
                            if (AssemblyNames.Count > 0)
                            {
                                Expression<Func<string, bool>> tmpWhere = x => false;
                                AssemblyNames.ToList().ForEach(aAssembly =>
                                {
                                    tmpWhere = tmpWhere.Or(x => x.Contains(aAssembly));
                                });

                                where = where.And(tmpWhere);
                            }

                            AssemblyPaths.SelectMany(x => Directory.GetFiles(x, "*.dll"))
                                .Where(x => where.Compile()(new FileInfo(x).Name))
                                .Distinct()
                                .Select(x =>
                                {
                                    try
                                    {
                                        return Assembly.LoadFrom(x);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                })
                                .Where(x => x != null && !x.IsDynamic)
                                .ForEach(aAssembly =>
                                {
                                    try
                                    {
                                        _allEntityTypes.AddRange(aAssembly.GetTypes());
                                    }

                                    catch
                                    {

                                    }
                                });
                        }
                    }
                }

                return _allEntityTypes;
            }
        }

        #endregion
    }
}
