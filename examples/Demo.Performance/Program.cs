using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Demo.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddEFCoreSharding(config =>
            {
                config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer);

                //添加数据源
                config.AddDataSource(Config.CONSTRING1, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);

                //对3取模分表
                config.SetHashModSharding<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
            });
            var serviceProvider = services.BuildServiceProvider();

            var db = serviceProvider.GetService<IDbAccessor>();
            var shardingDb = serviceProvider.GetService<IShardingDbAccessor>();
            Stopwatch watch = new Stopwatch();

            Expression<Func<Base_UnitTest, bool>> where = x => EF.Functions.Like(x.UserName, $"%00001C22-8DD2-4D47-B500-407554B099AB%");

            var q = db.GetIQueryable<Base_UnitTest>()
                .Where(where)
                .OrderByDescending(x => x.Id)
                .Skip(0)
                .Take(30);
            var shardingQ = shardingDb.GetIShardingQueryable<Base_UnitTest>()
                .Where(where)
                .OrderByDescending(x => x.Id)
                .Skip(0)
                .Take(30);

            //先执行一次预热
            q.ToList();
            shardingQ.ToList();

            watch.Restart();
            var list1 = q.ToList();
            watch.Stop();
            Console.WriteLine($"未分表耗时:{watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var list2 = shardingQ.ToList();
            watch.Stop();
            Console.WriteLine($"分表后耗时:{watch.ElapsedMilliseconds}ms");

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
