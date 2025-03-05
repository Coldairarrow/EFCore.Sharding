using EFCore.Sharding.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
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

            _ = DiagnosticListener.AllListeners.Subscribe(
                new DiagnosticObserver(serviceProvider.GetService<ILoggerFactory>(),
                _shardingOptions.MinCommandElapsedMilliseconds));

            Cache.RootServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            EFCoreShardingOptions.Bootstrapper?.Invoke(scope.ServiceProvider);

            //长时间未释放监控,5分钟
            _ = JobHelper.SetIntervalJob(() =>
            {
                System.Collections.Generic.List<GenericDbContext> list = Cache.DbContexts.Where(x => (DateTimeOffset.Now - x.CreateTime).TotalMinutes > 5).ToList();
                list.ForEach(x =>
                {
                    ILogger logger = x.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
                    logger?.LogWarning("DbContext长时间({ElapsedMinutes}m)未释放 CreateStackTrace:{CreateStackTrace} FirstCallStackTrace:{FirstCallStackTrace}",
                        (long)(DateTimeOffset.Now - x.CreateTime).TotalMinutes, x.CreateStackTrace, x.FirstCallStackTrace);
                });
            }, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }
    }
}
