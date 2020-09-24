using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class Startup
    {
        /// <summary>
        /// 所有单元测试开始前
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize]
        public static void Begin(TestContext context)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddEFCoreSharding(config =>
                    {
                        config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer);
                        config.UseDatabase<ISQLiteDb1>(Config.SQLITE1, DatabaseType.SQLite);
                        config.UseDatabase<ISQLiteDb2>(Config.SQLITE2, DatabaseType.SQLite);
                        config.UseDatabase<ICustomDbAccessor>(Config.CONSTRING1, DatabaseType.SqlServer);

                        //分表配置
                        //添加数据源
                        config.AddDataSource(Config.CONSTRING1, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);
                        //设置分表规则
                        config.SetHashModSharding<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
                    });
                }).Build();
            host.Start();

            RootServiceProvider = host.Services;
            ServiceScopeFactory = RootServiceProvider.GetService<IServiceScopeFactory>();
        }

        /// <summary>
        /// 所有单元测试结束后
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {

        }

        public static IServiceProvider RootServiceProvider { get; private set; }
        public static IServiceScopeFactory ServiceScopeFactory { get; private set; }
    }
}
