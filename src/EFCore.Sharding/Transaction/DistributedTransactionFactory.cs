namespace EFCore.Sharding
{
    /// <summary>
    /// 分布式事务工厂
    /// </summary>
    public static class DistributedTransactionFactory
    {
        /// <summary>
        /// 获取分布式事务
        /// </summary>
        /// <returns></returns>
        public static IDistributedTransaction GetDistributedTransaction()
        {
            return new DistributedTransaction();
        }
    }
}
