using EFCore.Sharding.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class Bootstrapper : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EFCoreShardingOptions _shardingOptions;
        public Bootstrapper(IServiceProvider serviceProvider, IOptions<EFCoreShardingOptions> shardingOptions)
        {
            _serviceProvider = serviceProvider;
            _shardingOptions = shardingOptions.Value;
            Cache.ServiceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _shardingOptions.Bootstrapper?.Invoke(_serviceProvider);

            return Task.CompletedTask;
        }
    }
}
