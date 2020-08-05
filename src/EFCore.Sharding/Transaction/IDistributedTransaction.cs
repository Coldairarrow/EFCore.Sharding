namespace EFCore.Sharding
{
    /// <summary>
    /// 分布式
    /// </summary>
    public interface IDistributedTransaction : ITransaction
    {
        /// <summary>
        /// 添加Db
        /// </summary>
        /// <param name="repositories"></param>
        void AddDbAccessor(params IDbAccessor[] repositories);
    }
}
