using EFCore.Sharding;
using EFCore.Sharding.Tests;
using System;
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
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.CONSTRING1)
                    .AddPhysicDbGroup()
                    .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
            });

            DateTime time1 = DateTime.Now;
            DateTime time2 = DateTime.Now;

            var db = DbFactory.GetDbAccessor(Config.CONSTRING1, DatabaseType.SqlServer);
            Stopwatch watch = new Stopwatch();

            var q = db.GetIQueryable<Base_UnitTest>()
                .Where(x => x.UserName.Contains("00001C22-8DD2-4D47-B500-407554B099AB"))
                .OrderByDescending(x => x.Id)
                .Skip(0)
                .Take(30);

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
