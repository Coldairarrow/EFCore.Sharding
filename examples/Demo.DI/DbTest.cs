using Demo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
                try
                {
                    while (true)
                    {
                        using (var scop = _serviceProvider.CreateScope())
                        {
                            var repository = scop.ServiceProvider.GetService<IMyRepository>();
                            await repository.InsertAsync(new Base_UnitTest
                            {
                                Id = Guid.NewGuid().ToString(),
                                Age = 100,
                                UserName = Guid.NewGuid().ToString()
                            });

                            _logger.LogInformation("插入数据成功");
                        }

                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                }

            }, TaskCreationOptions.LongRunning);

            await Task.CompletedTask;
        }
    }
}
