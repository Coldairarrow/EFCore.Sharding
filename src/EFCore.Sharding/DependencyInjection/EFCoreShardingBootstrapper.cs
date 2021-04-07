using EFCore.Sharding.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// EFCoreSharding初始化加载
    /// 注：非Host环境需要手动调用
    /// </summary>
    public class EFCoreShardingBootstrapper : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EFCoreShardingOptions _shardingOptions;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public EFCoreShardingBootstrapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _shardingOptions = serviceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;

            DiagnosticListener.AllListeners.Subscribe(
                new DiagnosticObserver(serviceProvider.GetService<ILoggerFactory>(), 
                _shardingOptions.MinCommandElapsedMilliseconds));

            Cache.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _shardingOptions.Bootstrapper?.Invoke(_serviceProvider);

            return Task.CompletedTask;
        }
    }
}
