using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                config.EnableComments(true);
                config.SetEntityAssemblies(typeof(Base_UnitTest).Assembly);

                config.UseDatabase(Config.SQLITE1, DatabaseType.SQLite);
            });
            var serviceProvider = services.BuildServiceProvider();
            new EFCoreShardingBootstrapper(serviceProvider).StartAsync(default).Wait();

            using var scop = serviceProvider.CreateScope();
            //拿到注入的IDbAccessor即可进行所有数据库操作
            var db = scop.ServiceProvider.GetService<IDbAccessor>();
            var logger = scop.ServiceProvider.GetService<ILogger<Program>>();
            var i = 0;

            var id = "e4e37e18-0784-4b82-bea0-633542ddf7a9";
            var u = new Base_UnitTestDto
            {
                Id = id,
                CreateTime = DateTime.Now,
            };
            var k = u;
            await db.UpdateAsync(k, ["CreateTime"]);

            //var u = await db.GetIQueryable<Base_UnitTest>().FirstOrDefaultAsync(v => v.Id == id);
            //u.CreateTime = DateTime.Now;
            //await db.UpdateAsync(u, [nameof(Base_UnitTest.CreateTime)]);




            //while (i < 10)
            //{
            //    var u = new Base_UnitTestDto
            //    {
            //        Age = 100,
            //        CreateTime = DateTime.Now,
            //        Id = Guid.NewGuid().ToString(),
            //        UserId = Guid.NewGuid().ToString(),
            //        UserName = Guid.NewGuid().ToString(),
            //        Key = Guid.NewGuid().ToString("N"),
            //    };
            //    await db.InsertAsync<Base_UnitTest>(u);
            //    var count = await db.GetIQueryable<Base_UnitTest>().CountAsync();

            //    logger.LogWarning("当前数量:{Count}", count);

            //    await Task.Delay(1000);
            //    i += 1;
            //}
        }

    }

    class Base_UnitTestDto : Base_UnitTest
    {
        public string Key { get; set; }
    }

}
