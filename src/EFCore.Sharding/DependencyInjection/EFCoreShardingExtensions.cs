using Microsoft.Extensions.DependencyInjection;
using System;

namespace EFCore.Sharding
{
    /// <summary>
    /// 拓展
    /// </summary>
    public static class EFCoreShardingExtensions
    {
        /// <summary>
        /// 注入EFCoreSharding
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="shardingBuilder">配置项</param>
        /// <returns></returns>
        public static IServiceCollection AddEFCoreSharding(this IServiceCollection services, Action<IShardingBuilder> shardingBuilder = null)
        {
            _ = services.AddOptions<EFCoreShardingOptions>();
            _ = services.AddLogging();

            ShardingContainer container = new(services);
            shardingBuilder?.Invoke(container);
            _ = services.AddSingleton(container);
            _ = services.AddSingleton<IShardingBuilder>(container);
            _ = services.AddSingleton<IShardingConfig>(container);
            _ = services.AddScoped<DbFactory>();
            _ = services.AddScoped<IDbFactory, DbFactory>();
            _ = services.AddScoped<IShardingDbAccessor, ShardingDbAccessor>();

            _ = services.AddHostedService<EFCoreShardingBootstrapper>();

            return services;
        }
    }
}
