namespace EFCore.Sharding
{
    /// <summary>
    /// 分表类型
    /// </summary>
    public enum ShardingType
    {
        /// <summary>
        /// 通过哈希取模分表
        /// </summary>
        HashMod,

        /// <summary>
        /// 按日期分表
        /// </summary>
        Date
    }
}
