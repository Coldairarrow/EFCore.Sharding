using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding
{
    internal interface IShardingConfig
    {
        DatabaseType FindADbType();
        bool LogicDelete { get; }
        string KeyField { get; }
        string DeletedField { get; }
        List<Type> AllEntityTypes { get; }
        List<(string suffix, string conString, DatabaseType dbType)> GetReadTables<T>(IQueryable<T> source);
        List<(string suffix, string conString, DatabaseType dbType)> GetWriteTables<T>(IQueryable<T> source = null);
        (string suffix, string conString, DatabaseType dbType) GetTheWriteTable<T>(T obj);
    }
}
