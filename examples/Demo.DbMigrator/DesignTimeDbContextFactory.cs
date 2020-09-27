using EFCore.Sharding;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Demo.DbMigrator
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomContext>
    {
        private static readonly string _connectionString = "Data Source=localhost;Initial Catalog=DbMigrator;Integrated Security=True";

        static DesignTimeDbContextFactory()
        {
            DateTime startTime = DateTime.Now.AddMinutes(-5);
            ServiceCollection services = new ServiceCollection();
            services.AddEFCoreSharding(x =>
            {
                x.MigrationsWithoutForeignKey();
                //添加数据源
                x.AddDataSource(_connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);

                //按分钟分表
                x.SetDateSharding<Order>(nameof(Order.CreateTime), ExpandByDateMode.PerMinute, startTime);
            });
            ServiceProvider = services.BuildServiceProvider();
            new EFCoreShardingBootstrapper(ServiceProvider).StartAsync(default).Wait();
        }

        public static readonly IServiceProvider ServiceProvider;

        /// <summary>
        /// 创建数据库上下文
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public CustomContext CreateDbContext(string[] args)
        {
            var db = ServiceProvider
                .GetService<IDbFactory>()
                .GetDbContext(new DbContextParamters
                {
                    ConnectionString = _connectionString,
                    DbType = DatabaseType.SqlServer,
                });

            return new CustomContext(db);
        }
    }
}
