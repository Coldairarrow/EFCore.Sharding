using System;

namespace EFCore.Sharding
{
    public interface IConfigInit
    {
        IConfigInit AddAbsDb(
            DatabaseType dbType,
            string absDbName = ShardingConfig.DefaultAbsDbName);
        IConfigInit AddPhysicDbGroup(
            string groupName = ShardingConfig.DefaultDbGourpName,
            string absDbName = ShardingConfig.DefaultAbsDbName);
        IConfigInit AddPhysicDb(
            ReadWriteType opType,
            string conString,
            string groupName = ShardingConfig.DefaultDbGourpName);
        IConfigInit AddPhysicTable<T>(
            string physicTableName,
            string groupName = ShardingConfig.DefaultDbGourpName);
        IConfigInit SetShardingRule<T>(
            IShardingRule<T> shardingRule,
            string absDbName = ShardingConfig.DefaultAbsDbName);
        IConfigInit AutoExpandByDate<T>(
            ExpandByDateMode expandByDateMode,
            Func<DateTime, string> formatTableName,
            Func<string, string> createTableSqlBuilder,
            string groupName = ShardingConfig.DefaultDbGourpName);
    }
}
