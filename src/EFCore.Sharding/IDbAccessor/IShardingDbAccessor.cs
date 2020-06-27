namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库分表操作接口
    /// </summary>
    public interface IShardingDbAccessor : IBaseDbAccessor
    {
        #region 查询数据

        /// <summary>
        /// 获取IShardingQueryable
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <returns></returns>
        IShardingQueryable<T> GetIShardingQueryable<T>() where T : class, new();

        #endregion
    }
}
