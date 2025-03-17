using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EFCore.Sharding
{
    /// <summary>
    /// 抽象提供器
    /// </summary>
    internal abstract class AbstractProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContextOptionsBuilder"></param>
        /// <param name="dbConnection"></param>
        public abstract void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract ModelBuilder GetModelBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDbContext"></param>
        /// <returns></returns>
        public abstract IDbAccessor GetDbAccessor(GenericDbContext baseDbContext);

        /// <summary>
        /// 
        /// </summary>
        public abstract DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbConnection GetDbConnection()
        {
            return DbProviderFactory.CreateConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbCommand GetDbCommand()
        {
            return DbProviderFactory.CreateCommand();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbParameter GetDbParameter()
        {
            return DbProviderFactory.CreateParameter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataAdapter GetDataAdapter()
        {
            return DbProviderFactory.CreateDataAdapter();
        }
    }
}
