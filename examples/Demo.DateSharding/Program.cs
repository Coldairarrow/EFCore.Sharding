using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Demo.DateSharding
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DateTime startTime = DateTime.Now.AddMinutes(-5);
            ServiceCollection services = new ServiceCollection();
            //配置初始化
            services.AddLogging(x =>
            {
                x.AddConsole();
            });
            services.AddEFCoreSharding(config =>
            {
                config.SetEntityAssemblies(typeof(Base_UnitTest).Assembly);

                //添加数据源
                config.AddDataSource(Config.CONSTRING1, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);

                //按分钟分表
                config.SetDateSharding<Base_UnitTest>(nameof(Base_UnitTest.CreateTime), ExpandByDateMode.PerMinute, startTime);
            });

            var serviceProvider = services.BuildServiceProvider();
            new EFCoreShardingBootstrapper(serviceProvider).StartAsync(default).Wait();

            using var scop = serviceProvider.CreateScope();

            var db = scop.ServiceProvider.GetService<IShardingDbAccessor>();
            var logger = scop.ServiceProvider.GetService<ILogger<Program>>();

            while (true)
            {
                try
                {
                    await db.InsertAsync(new Base_UnitTest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Age = 1,
                        UserName = Guid.NewGuid().ToString(),
                        CreateTime = DateTime.Now
                    });

                    DateTime time = DateTime.Now.AddMinutes(-2);
                    var count = await db.GetIShardingQueryable<Base_UnitTest>()
                        .Where(x => x.CreateTime >= time)
                        .CountAsync();
                    logger.LogWarning("当前数据量:{Count}", count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(1000);
            }
        }
    }
}
