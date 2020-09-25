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
            services.AddOptions<EFCoreShardingOptions>();
            services.AddLogging();

            ShardingContainer container = new ShardingContainer(services);
            shardingBuilder?.Invoke(container);

            services.AddSingleton<IShardingBuilder>(container);
            services.AddSingleton<IShardingConfig>(container);
            services.AddSingleton<DbFactory>();
            services.AddSingleton<IDbFactory, DbFactory>();
            services.AddScoped<IShardingDbAccessor, ShardingDbAccessor>();

            services.AddHostedService<Bootstrapper>();

            return services;
        }
    }
}
