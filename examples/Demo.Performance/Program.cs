using Demo.Common;
using EFCore.Sharding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Demo.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.ConString1)
                    .AddPhysicDbGroup()
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_0")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_1")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_2")
                    .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
            });

            DateTime time1 = DateTime.Now;
            DateTime time2 = DateTime.Now;

            var db = DbFactory.GetRepository(Config.ConString1, DatabaseType.SqlServer);
            Stopwatch watch = new Stopwatch();

            var q = db.GetIQueryable<Base_UnitTest>()
                .Where(x => x.UserName.Contains("aaa"))
                ;

            q.ToList();
            q.ToSharding().ToList();
            watch.Restart();
            var list1 = q.ToList();
            watch.Stop();
            Console.WriteLine($"未分表耗时:{watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var list2 = q.ToSharding().ToList();
            watch.Stop();
            Console.WriteLine($"分表后耗时:{watch.ElapsedMilliseconds}ms");

            Console.WriteLine("完成");

            Console.ReadLine();
        }
    }
}
