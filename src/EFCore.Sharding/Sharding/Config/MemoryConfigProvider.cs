using EFCore.Sharding.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding
{
    internal class MemoryConfigProvider : IConfigInit, IConfigProvider
    {
        #region 外部接口

        public List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName = "BaseDb")
        {
            return GetTargetTables(absTableName, ReadWriteType.Read, absDbName);
        }

        public (string tableName, string conString, DatabaseType dbType) GetTheWriteTable(string absTableName, object obj, string absDbName = "BaseDb")
        {
            return GetTargetTables(absTableName, ReadWriteType.Write, absDbName, obj).Single();
        }

        public List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables(string absTableName, string absDbName = null)
        {
            return GetTargetTables(absTableName, ReadWriteType.Write, absDbName, null);
        }

        public IConfigInit AddAbsDb(DatabaseType dbType, string absDbName = "BaseDb")
        {
            _absDbs.Add(new AbsDb
            {
                DbType = dbType,
                Name = absDbName
            });

            return this;
        }

        public IConfigInit AddPhysicDbGroup(string groupName = "BaseDbGroup", string absDbName = "BaseDb")
        {
            _physicDbGroups.Add(new PhysicDbGroup
            {
                AbsDbName = absDbName,
                GroupName = groupName
            });

            return this;
        }

        public IConfigInit AddPhysicDb(ReadWriteType opType, string conString, string groupName = "BaseDbGroup")
        {
            _physicDbs.Add(new PhysicDb
            {
                ConString = conString,
                GroupName = groupName,
                OpType = opType
            });

            return this;
        }

        public IConfigInit AddPhysicTable<T>(string physicTableName, string groupName = "BaseDbGroup")
        {
            var absEntityType = typeof(T);

            _physicTables.Add(new PhysicTable
            {
                AbsTableName = absEntityType.Name,
                GroupName = groupName,
                PhysicTableName = physicTableName
            });

            var physicEntityType = ShardingHelper.MapTable(absEntityType, physicTableName);
            DbModelFactory.AddEntityType(physicTableName, physicEntityType);

            return this;
        }

        public IConfigInit SetShardingRule<T>(IShardingRule<T> shardingRule, string absDbName = "BaseDb")
        {
            string key = $"{absDbName}.{typeof(T).Name}";
            _shardingRules[key] = obj =>
            {
                return shardingRule.FindTable(obj.ChangeType<T>());
            };

            return this;
        }

        public IConfigInit AutoExpandByDate<T>(
            ExpandByDateMode expandByDateMode,
            Func<DateTime, string> formatTableName,
            Func<string, string> createTableSqlBuilder,
            string groupName = ShardingConfig.DefaultDbGourpName)
        {
            (string conExpression, TimeSpan leadTime) paramter =
               expandByDateMode switch
               {
                   ExpandByDateMode.PerMinute => ("50 * * * * ? *", TimeSpan.FromSeconds(10)),
                   ExpandByDateMode.PerHour => ("0 50 * * * ? *", TimeSpan.FromMinutes(10)),
                   ExpandByDateMode.PerDay => ("0 0 23 * * ? *", TimeSpan.FromHours(1)),
                   ExpandByDateMode.PerMonth => ("0 0 0 L * ? *", TimeSpan.FromDays(1)),
                   ExpandByDateMode.PerYear => ("0 0 0 L 12 ? *", TimeSpan.FromDays(1)),
                   _ => throw new Exception("expandByDateMode参数无效")
               };

            JobHelper.SetCronJob(() =>
            {
                DateTime trueDate = DateTime.Now + paramter.leadTime;
                string tableName = formatTableName(trueDate);
                string sql = createTableSqlBuilder(tableName);
                var q = from a in _physicDbs
                        join b in _physicDbGroups on a.GroupName equals b.GroupName
                        join c in _absDbs on b.AbsDbName equals c.Name
                        where b.GroupName == groupName
                        select new
                        {
                            a.ConString,
                            c.DbType
                        };
                var dbList = q.ToList();
                //自动创建数据库表
                dbList.ForEach(aDb =>
                {
                    var repository = DbFactory.GetRepository(aDb.ConString, aDb.DbType);
                    repository.ExecuteSql(sql);
                });
                //添加表对应实体模型
                var physicEntityType = ShardingHelper.MapTable(typeof(T), tableName);
                DbModelFactory.AddEntityType(tableName, physicEntityType);
            }, paramter.conExpression);

            return this;
        }

        #endregion

        #region 私有成员

        private SynchronizedCollection<AbsDb> _absDbs { get; }
            = new SynchronizedCollection<AbsDb>();
        private SynchronizedCollection<PhysicDbGroup> _physicDbGroups { get; }
            = new SynchronizedCollection<PhysicDbGroup>();
        private SynchronizedCollection<PhysicDb> _physicDbs { get; }
            = new SynchronizedCollection<PhysicDb>();
        private SynchronizedCollection<PhysicTable> _physicTables { get; }
            = new SynchronizedCollection<PhysicTable>();
        private ConcurrentDictionary<string, Func<object, string>> _shardingRules
            = new ConcurrentDictionary<string, Func<object, string>>();
        private List<(string tableName, string conString, DatabaseType dbType)>
            GetTargetTables(string absTableName, ReadWriteType opType, string absDbName, object obj = null)
        {
            //获取数据库组
            var groupList = (from a in _physicTables
                             join b in _physicDbGroups on a.GroupName equals b.GroupName
                             join c in _absDbs on b.AbsDbName equals c.Name
                             where a.AbsTableName == absTableName && c.Name == absDbName
                             select new
                             {
                                 a.PhysicTableName,
                                 a.GroupName,
                                 c.DbType,
                                 a.AbsTableName,
                                 AbsDbName = c.Name
                             }).ToList();

            //若为写操作则只获取特定表
            if (!obj.IsNullOrEmpty())
            {
                string key = $"{absDbName}.{absTableName}";
                var shardingRule = _shardingRules[key];
                string tableName = shardingRule(obj);
                groupList = groupList.Where(x => x.PhysicTableName == tableName).ToList();
            }

            //数据库组中数据库负载均衡
            var resList = groupList.Select(x =>
            {
                var dbs = _physicDbs
                    .Where(x => x.GroupName == x.GroupName && x.OpType.HasFlag(opType))
                    .ToList();
                var theDb = RandomHelper.Next(dbs);

                return (x.PhysicTableName, theDb.ConString, x.DbType);
            }).ToList();

            return resList;
        }

        #endregion
    }
}
