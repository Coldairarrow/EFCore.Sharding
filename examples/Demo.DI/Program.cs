using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.DI
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(config =>
                {

                })
                .ConfigureServices((host, services) =>
                {
                    services.AddHostedService<DbTest>();
                    services.UseEFCoreSharding(config =>
                    {
                        config.UseDatabase(Config.SQLITE1, DatabaseType.SQLite);
                        //使用多个数据库
                        config.UseDatabase<IMyDbAccessor>(Config.SQLITE1, DatabaseType.SQLite);
                    });
                })
                .Build()
                .Run();
        }
    }
}
