using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding
{
    internal interface IConfigProvider
    {
        List<(string suffix, string conString, DatabaseType dbType)> GetReadTables(string absDbName, IQueryable source);

        List<(string suffix, string conString, DatabaseType dbType)> GetAllWriteTables<T>(string absDbName);

        (string suffix, string conString, DatabaseType dbType) GetTheWriteTable<T>(T obj, string absDbName);

        DatabaseType GetAbsDbType(string absDbName);
    }
}
