using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding
{
    internal interface IConfigProvider
    {
        List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName, IQueryable source);

        List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables<T>(string absDbName);

        (string tableName, string conString, DatabaseType dbType) GetTheWriteTable<T>(object obj, string absDbName);

        DatabaseType GetAbsDbType(string absDbName);
    }
}
