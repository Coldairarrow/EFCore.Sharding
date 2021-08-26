using Demo.DbMigrator.Entities;
using EFCore.Sharding;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Demo.DbMigrator
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomContext>
    {
        private static readonly string _connectionString = "Server=127.0.0.1;Port=5432;Database=EFCore.Sharding1;User Id=postgres;Password=postgres;";
        static DesignTimeDbContextFactory()
        {
            DateTime startTime = DateTime.Parse("2020/7/1");
            ServiceCollection services = new ServiceCollection();
            services.AddEFCoreSharding(x =>
            {
                x.SetEntityAssemblies(typeof(DesignTimeDbContextFactory).Assembly);
                x.EnableComments(true);

                //取消建表
                x.CreateShardingTableOnStarting(false);

                //取消外键
                x.MigrationsWithoutForeignKey();

                //使用分表迁移
                //x.EnableShardingMigration(true);

                //添加数据源
                //x.AddDataSource(_connectionString, ReadWriteType.Read | ReadWriteType.Write, DatabaseType.SqlServer);

                //按月分表
                //x.SetDateSharding<AuditLog>(nameof(Order.CreateTime), ExpandByDateMode.PerMonth, startTime);

                x.UseDatabase(_connectionString, DatabaseType.SqlServer);
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
                    DbType = DatabaseType.PostgreSql,
                });

            return new CustomContext(db);
        }
    }
}
