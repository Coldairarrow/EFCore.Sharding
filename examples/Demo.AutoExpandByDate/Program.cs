using Demo.Common;
using EFCore.Sharding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AutoExpandByDate
{
    class Base_UnitTestShardingRule : AbsShardingRule<Base_UnitTest>
    {
        public override DateTime BuildDate(Base_UnitTest obj)
        {
            return obj.CreateTime;
        }
    }

    class Program
    {
        /// <summary>
        /// 表都在同一个数据库中
        /// </summary>
        public static void OneGroup()
        {
            DateTime startTime = DateTime.Now.AddMinutes(-5);
            DateTime endTime = DateTime.MaxValue;

            //配置初始化
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)//添加抽象数据库
                    .AddPhysicDbGroup()//添加物理数据库组
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.ConString1)//添加物理数据库1
                    .SetShardingRule(new Base_UnitTestShardingRule())//设置分表规则
                    .AutoExpandByDate<Base_UnitTest>(//设置为按时间自动分表
                        ExpandByDateMode.PerMinute,
                        (startTime, endTime, ShardingConfig.DefaultDbGourpName)
                        );
            });
            var db = DbFactory.GetShardingRepository();
            while (true)
            {
                db.Insert(new Base_UnitTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Age = 1,
                    UserName = Guid.NewGuid().ToString(),
                    CreateTime = DateTime.Now
                });

                var count = db.GetIShardingQueryable<Base_UnitTest>().Count();
                Console.WriteLine($"当前数据量:{count}");

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 表分布在两个数据库测试
        /// </summary>
        public static void TwoGroup()
        {
            DateTime startTime1 = DateTime.Now.AddMinutes(-5);
            DateTime endTime1 = DateTime.Now.AddMinutes(5);
            DateTime startTime2 = endTime1;
            DateTime endTime2 = DateTime.MaxValue;

            string group1 = "group1";
            string group2 = "group2";

            //配置初始化
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)//添加抽象数据库
                    .AddPhysicDbGroup(group1)//添加物理数据库组1
                    .AddPhysicDbGroup(group2)//添加物理数据库组2
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.ConString1, group1)//添加物理数据库1
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.ConString2, group2)//添加物理数据库2
                    .SetShardingRule(new Base_UnitTestShardingRule())//设置分表规则
                    .AutoExpandByDate<Base_UnitTest>(//设置为按时间自动分表
                        ExpandByDateMode.PerMinute,
                        (startTime1, endTime1, group1),
                        (startTime2, endTime2, group2)
                        );
            });

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var db = DbFactory.GetShardingRepository();
                    while (true)
                    {
                        db.Insert(new Base_UnitTest
                        {
                            Id = Guid.NewGuid().ToString(),
                            Age = 1,
                            UserName = Guid.NewGuid().ToString(),
                            CreateTime = DateTime.Now
                        });

                        var count = db.GetIShardingQueryable<Base_UnitTest>().Count();
                        Console.WriteLine($"当前数据量:{count}");

                        Thread.Sleep(50);
                    }
                }));
            }

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            OneGroup();

            Console.ReadLine();
        }
    }
}
