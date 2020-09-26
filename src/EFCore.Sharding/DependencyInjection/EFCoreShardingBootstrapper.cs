using EFCore.Sharding.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    public class EFCoreShardingBootstrapper : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EFCoreShardingOptions _shardingOptions;
        public EFCoreShardingBootstrapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _shardingOptions = serviceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;

            Cache.ServiceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _shardingOptions.Bootstrapper?.Invoke(_serviceProvider);

            return Task.CompletedTask;
        }
    }
}
