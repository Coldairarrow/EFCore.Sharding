using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库提供抽象类
    /// </summary>
    public abstract class AbstractProvider
    {
        /// <summary>
        /// 使用某个数据库
        /// </summary>
        /// <param name="dbContextOptionsBuilder">dbContextOptionsBuilder</param>
        /// <param name="dbConnection">连接字符串</param>
        public abstract void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection);

        /// <summary>
        /// 获取ModelBuilder
        /// </summary>
        /// <returns></returns>
        public abstract ModelBuilder GetModelBuilder();

        /// <summary>
        /// 获取仓储接口
        /// </summary>
        /// <param name="conString">完整数据库连接字符串</param>
        /// <returns></returns>
        public abstract IRepository GetRepository(string conString);

        /// <summary>
        /// 提供工厂
        /// </summary>
        public abstract DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        /// 获取DbConnection
        /// </summary>
        /// <returns></returns>
        public DbConnection GetDbConnection() => DbProviderFactory.CreateConnection();

        /// <summary>
        /// 获取DbCommand
        /// </summary>
        /// <returns></returns>
        public DbCommand GetDbCommand() => DbProviderFactory.CreateCommand();

        /// <summary>
        /// 获取DbParameter
        /// </summary>
        /// <returns></returns>
        public DbParameter GetDbParameter() => DbProviderFactory.CreateParameter();

        /// <summary>
        /// 获取DataAdapter
        /// </summary>
        /// <returns></returns>
        public DataAdapter GetDataAdapter() => DbProviderFactory.CreateDataAdapter();
    }
}
