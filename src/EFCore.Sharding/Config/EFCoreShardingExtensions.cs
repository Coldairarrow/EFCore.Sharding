using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
        public static IServiceCollection AddEFCoreSharding(this IServiceCollection services, Action<IShardingBuilder> shardingBuilder)
        {
            ShardingContainer container = new ShardingContainer(services);
            if (!services.Any(x => x.ServiceType == typeof(ILoggerFactory)))
            {
                services.AddLogging();
            }

            services.AddSingleton<IShardingConfig>(container);
            services.AddSingleton<IShardingBuilder>(container);

            services.AddSingleton<IDbFactory, DbFactory>();

            services.AddScoped<IShardingDbAccessor, ShardingDbAccessor>();

            shardingBuilder(container);

            return services;
        }
    }
}
