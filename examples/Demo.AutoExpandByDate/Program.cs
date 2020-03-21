using Coldairarrow.Util;
using EFCore.Sharding;
using System;
using System.IO;
using System.Threading;

namespace Demo.AutoExpandByDate
{
    class Base_UnitTestShardingRule : AbsShardingRule<Base_UnitTest>
    {
        public override string BuildTableSuffix(Base_UnitTest obj)
        {
            var time = new SnowflakeId(Convert.ToInt64(obj.Id)).Time;

            return time.ToString("yyyyMMddHHmm");
        }
    }

    class Program
    {
        static Program()
        {
            new IdHelperBootstrapper().SetWorkderId(1).Boot();
        }
        static void Main(string[] args)
        {
            DateTime startTime = Convert.ToDateTime("21:47:00");
            string conString = "Data Source=.;Initial Catalog=Colder.Admin.AntdVue;Integrated Security=True";

            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, conString)
                    .AddPhysicDbGroup()
                    .SetShardingRule(new Base_UnitTestShardingRule())
                    .AutoExpandByDate<Base_UnitTest>(
                        startTime,
                        ExpandByDateMode.PerMinute,
                        time => $"{typeof(Base_UnitTest).Name}_{time.ToString("yyyyMMddHHmm")}",
                        tableName => File.ReadAllText("Base_UnitTest.sql").Replace("Base_UnitTest", tableName)
                        );
            });

            var db = DbFactory.GetShardingRepository();
            while (true)
            {
                if (DateTime.Now > startTime.AddSeconds(5))
                {
                    db.Insert(new Base_UnitTest
                    {
                        Id = IdHelper.GetId(),
                        Age = 1,
                        UserId = IdHelper.GetId(),
                        UserName = IdHelper.GetId()
                    });

                    var count = db.GetIShardingQueryable<Base_UnitTest>().Count();
                    Console.WriteLine($"当前数据量:{count}");
                }

                Thread.Sleep(50);
            }
        }
    }
}
