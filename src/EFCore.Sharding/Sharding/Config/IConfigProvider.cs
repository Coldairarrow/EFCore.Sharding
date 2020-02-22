using System.Collections.Generic;

namespace EFCore.Sharding
{
    internal interface IConfigProvider
    {
        /// <summary>
        /// 获取读表
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName);

        /// <summary>
        /// 获取所有的写表
        /// </summary>
        /// <param name="absTableName">抽象表名</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables(string absTableName, string absDbName);

        /// <summary>
        /// 获取特定写表
        /// </summary>
        /// <param name="absTableName">抽象表名</param>
        /// <param name="obj">实体对象</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        (string tableName, string conString, DatabaseType dbType) GetTheWriteTable(string absTableName, object obj, string absDbName);
    }
}
