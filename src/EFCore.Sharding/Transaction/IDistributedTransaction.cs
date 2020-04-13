namespace EFCore.Sharding
{
    public interface IDistributedTransaction : ITransaction
    {
        void AddRepository(params IRepository[] repositories);
    }
}
