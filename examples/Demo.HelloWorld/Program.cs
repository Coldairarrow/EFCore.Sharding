using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
                config.SetEntityAssemblies(typeof(Base_UnitTest).Assembly);

                config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer);
            });
            var serviceProvider = services.BuildServiceProvider();
            new EFCoreShardingBootstrapper(serviceProvider).StartAsync(default).Wait();

            using var scop = serviceProvider.CreateScope();
            //拿到注入的IDbAccessor即可进行所有数据库操作
            var db = scop.ServiceProvider.GetService<IDbAccessor>();
            var logger = scop.ServiceProvider.GetService<ILogger<Program>>();

            while (true)
            {
                await db.InsertAsync(new Base_UnitTest
                {
                    Age = 100,
                    CreateTime = DateTime.Now,
                    Id = Guid.NewGuid().ToString(),
                    UserId = Guid.NewGuid().ToString(),
                    UserName = Guid.NewGuid().ToString()
                });
                var count = await db.GetIQueryable<Base_UnitTest>().CountAsync();
                
                var data = await db.GetIQueryable<Base_UnitTest>().ToListAsync();

                logger.LogWarning("当前数量:{Count}", count);

                //await Task.Delay(1000);
            }
        }
    }
}
