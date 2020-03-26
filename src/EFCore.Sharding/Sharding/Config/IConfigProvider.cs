using System.Collections.Generic;

namespace EFCore.Sharding
{
    internal interface IConfigProvider
    {
        List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName);

        List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables(string absTableName, string absDbName);

        (string tableName, string conString, DatabaseType dbType) GetTheWriteTable(string absTableName, object obj, string absDbName);
    }
}
