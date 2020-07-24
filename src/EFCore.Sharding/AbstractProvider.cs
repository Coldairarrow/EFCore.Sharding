using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EFCore.Sharding
{
    internal abstract class AbstractProvider
    {
        public abstract void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection, GenericDbContextOptions options);
        public abstract ModelBuilder GetModelBuilder();
        public abstract IDbAccessor GetDbAccessor(GenericDbContext baseDbContext);
        public abstract DbProviderFactory DbProviderFactory { get; }
        public DbConnection GetDbConnection() => DbProviderFactory.CreateConnection();
        public DbCommand GetDbCommand() => DbProviderFactory.CreateCommand();
        public DbParameter GetDbParameter() => DbProviderFactory.CreateParameter();
        public DataAdapter GetDataAdapter() => DbProviderFactory.CreateDataAdapter();
    }
}
