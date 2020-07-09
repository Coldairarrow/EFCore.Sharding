using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Demo.DI
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.UseEFCoreSharding(config =>
            {
                //单表
                config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer, "Demo.DI");
                //使用多个数据库,相同实体名不同命名空间
                config.UseDatabase<IMyDbAccessor>(Config.CONSTRING2, DatabaseType.SqlServer, "EFCore.Sharding.Tests");

                DateTime startTime = DateTime.Now.AddMinutes(-5);
                DateTime endTime = DateTime.MaxValue;
                //分表
                config.AddAbsDb(DatabaseType.SqlServer)//添加抽象数据库
                    .AddPhysicDbGroup()//添加物理数据库组
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.CONSTRING1)//添加物理数据库1
                    .SetDateShardingRule<Base_UnitTest>(nameof(Base_UnitTest.CreateTime))//设置分表规则
                    .AutoExpandByDate<Base_UnitTest>(//设置为按时间自动分表
                        ExpandByDateMode.PerMinute,
                        (startTime, endTime, ShardingConfig.DefaultDbGourpName)
                        );
            });
            var serviceProvider = services.BuildServiceProvider();

            using (var scop = serviceProvider.CreateScope())
            {
                var db1 = scop.ServiceProvider.GetService<IDbAccessor>();
                Console.WriteLine($"db1数量:{db1.GetIQueryable<Base_UnitTest>().Count()}");

                var db2 = scop.ServiceProvider.GetService<IMyDbAccessor>();
                db2.GetIQueryable<EFCore.Sharding.Tests.Base_UnitTest>().Count();

                Console.WriteLine($"db2数量:{db2.GetIQueryable<EFCore.Sharding.Tests.Base_UnitTest>().Count()}");
            }

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
