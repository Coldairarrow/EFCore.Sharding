using Coldairarrow.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class Startup
    {
        private const string CONNECTION_STRING = "DataSource=db.db";

        /// <summary>
        /// 所有单元测试开始前
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize]
        public static void Begin(TestContext context)
        {
            InitId();
            ServiceCollection services = new ServiceCollection();
            services.UseEFCoreSharding(config =>
            {
                config.UseDatabase(CONNECTION_STRING, DatabaseType.SQLite);
                config.UseDatabase<ICustomRepository>(CONNECTION_STRING, DatabaseType.SQLite);
                config.AddAbsDb(DatabaseType.SQLite)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, CONNECTION_STRING)
                    .AddPhysicDbGroup()
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_0")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_1")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_2")
                    .SetShardingRule(new Base_UnitTestShardingRule());
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 所有单元测试结束后
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {
            //结束后
        }

        public static IServiceProvider ServiceProvider;

        private static void InitId()
        {
            new IdHelperBootstrapper().SetWorkderId(1).Boot();
        }
    }
}
