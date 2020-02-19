using System;

namespace EFCore.Sharding
{
    /// <summary>
    /// 配置
    /// </summary>
    public static class ShardingConfig
    {
        /// <summary>
        /// 默认抽象数据库名
        /// </summary>
        public const string DefaultAbsDbName = "BaseDb";

        /// <summary>
        /// 默认数据库组名
        /// </summary>
        public const string DefaultDbGourpName = "BaseDbGroup";

        /// <summary>
        /// 初始化,只需程序启动执行一次
        /// </summary>
        /// <param name="configInit">初始化操作</param>
        public static void Init(Action<IConfigInit> configInit)
        {
            if (_inited)
                throw new Exception("只能初始化一次");
            _inited = true;

            MemoryConfigProvider memoryConfigProvider = new MemoryConfigProvider();
            configInit(memoryConfigProvider);

            ConfigProvider = memoryConfigProvider;
        }

        internal static void CheckInit()
        {
            if (!_inited)
                throw new Exception("未配置相关参数,请使用ShardingConfig.Init初始化");
        }

        private static bool _inited = false;
        internal static IConfigProvider ConfigProvider { get; set; }
    }
}
