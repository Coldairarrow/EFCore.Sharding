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
        /// <param name="absDbName">抽象数据库</param>
        /// <returns>IShardingQueryable</returns>
        public static IShardingQueryable<T> ToSharding<T>(this IQueryable<T> source, string absDbName = ShardingConfig.DefaultAbsDbName) where T : class, new()
        {
            ShardingConfig.CheckInit();

            return new ShardingQueryable<T>(source, DbFactory.GetShardingDbAccessor(absDbName) as ShardingDbAccessor, absDbName);
        }
    }
}
