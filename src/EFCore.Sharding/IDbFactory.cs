namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public interface IDbFactory
    {
        /// <summary>
        /// 获取DbAccessor
        /// </summary>
        /// <param name="dbContextParamters">参数</param>
        /// <param name="optionName">选项名</param>
        /// <returns></returns>
        IDbAccessor GetDbAccessor(DbContextParamters dbContextParamters, string optionName = null);

        /// <summary>
        /// 获取DbContext
        /// </summary>
        /// <param name="dbContextParamters">参数</param>
        /// <param name="eFCoreShardingOptions">参数</param>
        /// <returns></returns>
        GenericDbContext GetDbContext(DbContextParamters dbContextParamters, EFCoreShardingOptions eFCoreShardingOptions = null);
    }
}
