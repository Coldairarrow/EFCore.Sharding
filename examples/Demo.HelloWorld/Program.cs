using Demo.DbMigrator;
using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.HelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(config =>
            {
                config.AddConsole();
            });
            services.AddEFCoreSharding(config =>
            {
                config.SetMinCommandElapsedMilliseconds(0);
                config.SetEntityAssemblies(typeof(Base_UnitTest).Assembly, typeof(Order).Assembly);

                config.UseDatabase("Server=127.0.0.1;Port=5432;Database=EFCore.Sharding1;User Id=postgres;Password=postgres;", DatabaseType.PostgreSql);
            });
            var serviceProvider = services.BuildServiceProvider();
            new EFCoreShardingBootstrapper(serviceProvider).StartAsync(default).Wait();

            using var scop = serviceProvider.CreateScope();
            //拿到注入的IDbAccessor即可进行所有数据库操作
            var db = scop.ServiceProvider.GetService<IDbAccessor>();
            //var theData = await db.GetIQueryable<Order>().AsTracking().Where(x => x.Id == "0").FirstOrDefaultAsync();
            //theData.Name = "22222222";
            //await db.SaveChangesAsync();

            //{7026e1b9-7d5c-4fb0-8bd4-fbc9eaa75f63,7ec6420f-923e-49dd-9e50-2beea328ed85}

            //47759f8c-5620-4e4a-a6db-b783d547e343
            //var ids = new string[] { "47759f8c-5620-4e4a-a6db-b783d547e343", "7ec6420f-923e-49dd-9e50-2beea328ed85" };
            //var ids = new int[] { 1, 2 };

            var list = await db.GetIQueryable<Order>().Where(x => !x.Tags.Contains("5000")).ToListAsync();

            list = await db.GetIQueryable<Order>().Where(x => !x.Tags.Contains("5000")).ToListAsync();

            list = await db.GetIQueryable<Order>().Where(x => !x.Tags.Contains("5000")).ToListAsync();

            //List<Order> insertList = new List<Order>();
            //for (int i = 0; i < 10000; i++)
            //{
            //    insertList.Add(new Order
            //    {
            //        Id = i.ToString(),
            //        Tags = new string[] { i.ToString() },
            //        Tags2 = new string[] { i.ToString() }
            //    });
            //}

            //await db.InsertAsync(insertList);

            Console.WriteLine("OK");
        }
    }
}
