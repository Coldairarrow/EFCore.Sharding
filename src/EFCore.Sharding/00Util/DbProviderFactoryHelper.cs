//using System;
//using System.Data.Common;
//using System.Data.SqlClient;

//namespace EFCore.Sharding.Util
//{
//    /// <summary>
//    /// 数据库操作提供源工厂帮助类
//    /// </summary>
//    internal class DbProviderFactoryHelper
//    {
//        #region 外部接口

//        /// <summary>
//        /// 获取提供工厂
//        /// </summary>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DbProviderFactory GetDbProviderFactory(DatabaseType dbType)
//        {
//            DbProviderFactory factory = null;
//            switch (dbType)
//            {
//                case DatabaseType.SqlServer: factory = SqlClientFactory.Instance; break;
//                case DatabaseType.MySql: factory = MySqlClientFactory.Instance; break;
//                case DatabaseType.PostgreSql: factory = NpgsqlFactory.Instance; break;
//                case DatabaseType.Oracle: factory = OracleClientFactory.Instance; break;
//                case DatabaseType.SQLite: factory = SqliteFactory.Instance; break;
//                default: throw new Exception("请传入有效的数据库！");
//            }

//            return factory;
//        }

//        /// <summary>
//        /// 获取DbConnection
//        /// </summary>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DbConnection GetDbConnection(DatabaseType dbType)
//        {
//            var con = GetDbProviderFactory(dbType).CreateConnection();

//            return con;
//        }

//        /// <summary>
//        /// 获取DbCommand
//        /// </summary>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DbCommand GetDbCommand(DatabaseType dbType)
//        {
//            return GetDbProviderFactory(dbType).CreateCommand();
//        }

//        /// <summary>
//        /// 获取DbParameter
//        /// </summary>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DbParameter GetDbParameter(DatabaseType dbType)
//        {
//            return GetDbProviderFactory(dbType).CreateParameter();
//        }

//        /// <summary>
//        /// 获取DataAdapter
//        /// </summary>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DataAdapter GetDataAdapter(DatabaseType dbType)
//        {
//            return GetDbProviderFactory(dbType).CreateDataAdapter();
//        }

//        /// <summary>
//        /// 获取数据库连接对象
//        /// </summary>
//        /// <param name="conStr">连接字符串</param>
//        /// <param name="dbType">数据库类型</param>
//        /// <returns></returns>
//        public static DbConnection GetDbConnection(string conStr, DatabaseType dbType)
//        {
//            DbConnection dbConnection = GetDbConnection(dbType);
//            dbConnection.ConnectionString = conStr;

//            return dbConnection;
//        }

//        #endregion
//    }
//}
