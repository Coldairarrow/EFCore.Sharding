namespace EFCore.Sharding
{
    /// <summary>
    /// 按时间扩容模式
    /// </summary>
    public enum ExpandByDateMode
    {
        /// <summary>
        /// 每分钟
        /// </summary>
        PerMinute,

        /// <summary>
        /// 每小时
        /// </summary>
        PerHour,

        /// <summary>
        /// 每天
        /// </summary>
        PerDay,

        /// <summary>
        /// 每月
        /// </summary>
        PerMonth,

        /// <summary>
        /// 每年
        /// </summary>
        PerYear
    }
}
