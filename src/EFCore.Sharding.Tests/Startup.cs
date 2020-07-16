using Microsoft.Extensions.DependencyInjection;
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
            ServiceCollection services = new ServiceCollection();
            services.AddEFCoreSharding(config =>
            {
                config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer);
                config.UseDatabase<ISQLiteDb1>(Config.SQLITE1, DatabaseType.SQLite);
                config.UseDatabase<ISQLiteDb2>(Config.SQLITE2, DatabaseType.SQLite);
                config.UseDatabase<ICustomDbAccessor>(Config.CONSTRING1, DatabaseType.SqlServer);
                config.AddAbsDb(DatabaseType.SqlServer)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.CONSTRING1)
                    .AddPhysicDbGroup()
                    .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 所有单元测试结束后
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {

        }

        public static IServiceProvider ServiceProvider;
    }
}
