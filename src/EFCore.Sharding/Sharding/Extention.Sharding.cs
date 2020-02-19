using System.Linq;

namespace EFCore.Sharding
{
    /// <summary>
    /// 拓展
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 转为Sharding
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IShardingQueryable<T> ToSharding<T>(this IQueryable<T> source) where T : class, new()
        {
            ShardingConfig.CheckInit();

            return new ShardingQueryable<T>(source);
        }

        /// <summary>
        /// 转为Sharding
        /// </summary>
        /// <param name="db">数据源</param>
        /// <returns></returns>
        public static IShardingRepository ToSharding(this IRepository db)
        {
            ShardingConfig.CheckInit();

            return new ShardingRepository(db);
        }
    }
}
