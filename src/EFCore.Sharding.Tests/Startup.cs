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
            services.UseEFCoreSharding(config =>
            {
                config.UseDatabase(Config.SQLITE1, DatabaseType.SQLite);
                config.UseDatabase<ICustomDbAccessor>(Config.SQLITE1, DatabaseType.SQLite);
                config.AddAbsDb(DatabaseType.SQLite)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.SQLITE1)
                    .AddPhysicDbGroup()
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_0")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_1")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_2");
                    //.SetShardingRule(new Base_UnitTestShardingRule())
                    //.AddPhysicTable<SqlDefaultTestModel>("sql_default_test");

                DateTime startTime = DateTime.Now.AddMinutes(-5);
                DateTime endTime = DateTime.MaxValue;
                config.AddAbsDb(DatabaseType.PostgreSql, absDbName: "postgres")
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, "server=localhost;uid=postgres;password=;database=test;port=5433;commandtimeout=1024;", groupName: "postgres")
                    .AddPhysicDbGroup(groupName: "postgres", absDbName: "postgres")
                    .SetDateShardingRule<SqlDefaultTestModel>(nameof(SqlDefaultTestModel.ModifiedOn), absDbName: "postgres")//设置分表规则
                    .AutoExpandByDate<SqlDefaultTestModel>(//设置为按时间自动分表
                        ExpandByDateMode.PerMonth,
                        (startTime, endTime, groupName: "postgres")
                        );
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
