using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.DI
{
    class DbTest : BackgroundService
    {
        readonly IServiceProvider _serviceProvider;
        readonly ILogger _logger;
        public DbTest(IServiceProvider serviceProvider, ILogger<DbTest> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        using (var scop = _serviceProvider.CreateScope())
                        {
                            //单表
                            var db = scop.ServiceProvider.GetService<IMyDbAccessor>();
                            List<Base_UnitTest> insertList = new List<Base_UnitTest>();
                            for (int i = 0; i < 100; i++)
                            {
                                insertList.Add(new Base_UnitTest
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Age = i,
                                    CreateTime = DateTime.Now,
                                    UserName = Guid.NewGuid().ToString()
                                });
                            }

                            var single = new Base_UnitTest
                            {
                                Id = Guid.NewGuid().ToString(),
                                Age = 100,
                                CreateTime = DateTime.Now,
                                UserName = Guid.NewGuid().ToString()
                            };

                            await db.InsertAsync(single);
                            await db.InsertAsync(insertList);

                            int count = await db.GetIQueryable<Base_UnitTest>().CountAsync();
                            _logger.LogInformation("单表插入数据成功 当前数据量:{Count}", count);

                            //分表
                            var shardingDb = scop.ServiceProvider.GetService<IShardingDbAccessor>();
                            await shardingDb.InsertAsync(single);
                            await shardingDb.InsertAsync(insertList);
                            count = await shardingDb.GetIShardingQueryable<Base_UnitTest>().CountAsync();
                            _logger.LogInformation("分表插入数据成功 当前数据量:{Count}", count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "");
                    }

                    await Task.Delay(2000);
                }

            }, TaskCreationOptions.LongRunning);

            await Task.CompletedTask;
        }
    }
}
