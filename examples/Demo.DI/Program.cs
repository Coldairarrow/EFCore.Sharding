using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
                        //单表
                        config.UseDatabase(Config.CONSTRING1, DatabaseType.SqlServer);
                        //使用多个数据库
                        config.UseDatabase<IMyDbAccessor>(Config.CONSTRING1, DatabaseType.SqlServer);

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
                })
                .Build()
                .Run();
        }
    }
}
