using Coldairarrow.Util;
using EFCore.Sharding;
using System;
using System.Threading;

namespace Demo.AutoExpandByDate
{
    class Base_UnitTestShardingRule : AbsShardingRule<Base_UnitTest>
    {
        public override DateTime BuildDate(Base_UnitTest obj)
        {
            return new SnowflakeId(Convert.ToInt64(obj.Id)).Time;
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
            DateTime startTime = Convert.ToDateTime("21:05:00");
            string conString = "Data Source=.;Initial Catalog=Colder.Admin.AntdVue;Integrated Security=True";

            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, conString)
                    .AddPhysicDbGroup()
                    .SetShardingRule(new Base_UnitTestShardingRule())
                    .AutoExpandByDate<Base_UnitTest>(startTime, ExpandByDateMode.PerMinute);
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
