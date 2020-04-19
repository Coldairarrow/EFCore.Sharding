using Coldairarrow.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    class Startup
    {
        /// <summary>
        /// 所有单元测试开始前
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize]
        public static void Begin(TestContext context)
        {
            InitId();
            InitSharding();
        }

        /// <summary>
        /// 所有单元测试结束后
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {
            //结束后
        }

        private static void InitId()
        {
            new IdHelperBootstrapper()
                //设置WorkerId
                .SetWorkderId(1)
                //使用Zookeeper
                //.UseZookeeper("127.0.0.1:2181", 200, GlobalSwitch.ProjectName)
                .Boot();
        }
        private static void InitSharding()
        {
ShardingConfig.Init(config =>
{
    config.AddAbsDb(DatabaseType.SQLite)
        .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, "DataSource=db.db")
        .AddPhysicDbGroup()
        .AddPhysicTable<Base_UnitTest>("Base_UnitTest_0")
        .AddPhysicTable<Base_UnitTest>("Base_UnitTest_1")
        .AddPhysicTable<Base_UnitTest>("Base_UnitTest_2")
        .SetShardingRule(new Base_UnitTestShardingRule());
});
        }
    }
}
