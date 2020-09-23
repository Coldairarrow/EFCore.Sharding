using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Demo.ReadWrite
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
                //SQLITE1作为主库(写库)
                //SQLITE2作为从库(读库)
                config.UseDatabase(new (string, ReadWriteType)[]
                {
                    (Config.SQLITE1, ReadWriteType.Write),
                    (Config.SQLITE2, ReadWriteType.Read)
                }, DatabaseType.SQLite);
            });
            var serviceProvider = services.BuildServiceProvider();

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

                //注意:这里数量始终为0,因为SQLITE1与SQLITE2没有开启主从复制
                //在实际使用中应在数据库层开启主从复制
                logger.LogWarning("当前数量:{Count}", count);

                await Task.Delay(1000);
            }
        }
    }
}
