using Demo.Common;
using EFCore.Sharding;
using System;
using System.Threading;

namespace Demo.AutoExpandByDate
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now.AddMinutes(-5);
            DateTime endTime = DateTime.MaxValue;

            //配置初始化
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)//添加抽象数据库
                    .AddPhysicDbGroup()//添加物理数据库组
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.ConString1)//添加物理数据库1
                    .SetDateShardingRule<Base_UnitTest>(nameof(Base_UnitTest.CreateTime))//设置分表规则
                    .AutoExpandByDate<Base_UnitTest>(//设置为按时间自动分表
                        ExpandByDateMode.PerMinute,
                        (startTime, endTime, ShardingConfig.DefaultDbGourpName)
                        );
            });
            var db = DbFactory.GetShardingRepository();
            while (true)
            {
                try
                {
                    db.Insert(new Base_UnitTest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Age = 1,
                        UserName = Guid.NewGuid().ToString(),
                        CreateTime = DateTime.Now
                    });

                    DateTime time = DateTime.Now.AddMinutes(-2);
                    var count = db.GetIShardingQueryable<Base_UnitTest>()
                        .Where(x => x.CreateTime >= time)
                        .Count();
                    Console.WriteLine($"当前数据量:{count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(50);
            }

            Console.ReadLine();
        }
    }
}
