using EFCore.Sharding;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DbMigrator
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomContext>, IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 创建数据库上下文
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public CustomContext CreateDbContext(string[] args)
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddEFCoreSharding(x =>
                    {
                        x.MigrationsWithoutForeignKey();
                    });
                }).Build();
            host.Start();

            var db = host.Services
                .GetService<IDbFactory>()
                .GetDbContext(new DbContextParamters
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=DbMigrator;Integrated Security=True",
                    DbType = DatabaseType.SqlServer,
                });

            return new CustomContext(db);
        }
    }
}
