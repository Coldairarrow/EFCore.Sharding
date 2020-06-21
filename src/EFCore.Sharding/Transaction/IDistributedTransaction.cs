namespace EFCore.Sharding
{
    public interface IDistributedTransaction : ITransaction
    {
        void AddDbAccessor(params IDbAccessor[] repositories);
    }
}
