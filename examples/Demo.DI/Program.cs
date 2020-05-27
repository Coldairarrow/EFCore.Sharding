using EFCore.Sharding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.DI
{
    class Program
    {
        static void Main(string[] args)
        {
            string conString = "DataSource=db.db";

            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(config =>
                {

                })
                .ConfigureServices((host, services) =>
                {
                    services.AddHostedService<DbTest>();
                    services.UseEFCoreSharding(config =>
                    {
                        config.UseDatabase(conString, DatabaseType.SQLite);
                        //使用多个数据库
                        config.UseDatabase<IMyRepository>(conString, DatabaseType.SQLite);
                    });
                })
                .Build()
                .Run();
        }
    }
}
