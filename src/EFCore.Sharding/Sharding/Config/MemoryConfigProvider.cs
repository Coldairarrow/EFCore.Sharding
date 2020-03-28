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

        public List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName)
        {
            return GetTargetTables(absTableName, ReadWriteType.Read, absDbName);
        }

        public (string tableName, string conString, DatabaseType dbType) GetTheWriteTable(string absTableName, object obj, string absDbName)
        {
            return GetTargetTables(absTableName, ReadWriteType.Write, absDbName, obj).Single();
        }

        public List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables(string absTableName, string absDbName)
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

        public IConfigInit AddPhysicTable<TEntity>(string physicTableName, string groupName = "BaseDbGroup")
        {
            var absEntityType = typeof(TEntity);

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

        public IConfigInit SetShardingRule<TEntity>(AbsShardingRule<TEntity> shardingRule, string absDbName = "BaseDb")
        {
            string absTableName = typeof(TEntity).Name;
            string key = $"{absDbName}.{absTableName}";
            _shardingRules[key] = obj =>
            {
                var targetObj = obj.ChangeType<TEntity>();
                var tableName = absTableName;

                string suffix = string.Empty;
                //日期
                var date = shardingRule.BuildDate(targetObj);
                if (date != DateTime.MinValue)
                {
                    string tableKey = GetTableKey(absDbName, absTableName);
                    var expandMode = _expandByDateMode[tableKey];
                    suffix = date.ToString(GetDateFormat(expandMode));

                    return $"{absTableName}_{suffix}";
                }

                //后缀
                suffix = shardingRule.BuildTableSuffix(targetObj);
                if (!suffix.IsNullOrEmpty())
                    return $"{absTableName}_{suffix}";
                tableName = $"{absTableName}_{suffix}";
                //全名
                tableName = shardingRule.BuildTableName(obj.ChangeType<TEntity>());

                return tableName;
            };

            return this;
        }

        public IConfigInit AutoExpandByDate<TEntity>(
            DateTime startTime,
            ExpandByDateMode expandByDateMode,
            string groupName = ShardingConfig.DefaultDbGourpName)
        {
            string absTableName = typeof(TEntity).Name;
            string absDbName = _physicDbGroups.Where(x => x.GroupName == groupName).FirstOrDefault()?.AbsDbName;
            if (absDbName.IsNullOrEmpty())
                throw new Exception("缺少抽象数据库与物理数据库组信息");
            _expandByDateMode[GetTableKey(absDbName, absTableName)] = expandByDateMode;

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

            var theTime = startTime;


            while (true)
            {
                if (theTime > DateTime.Now)
                    break;
                string tableName = BuildTableName(theTime);
                AddPhysicTable<TEntity>(tableName, groupName);
                //建表
                //自动创建数据库表
                GetGroupDbs(groupName).ForEach(aDb =>
                {
                    var entityType = ShardingHelper.MapTable(typeof(TEntity), tableName);
                    DbFactory.CreateTable(aDb.ConString, aDb.DbType, entityType);
                });

                var key = expandByDateMode.ToString().Replace("Per", "");
                var method = theTime.GetType().GetMethod($"Add{key}s");
                theTime = (DateTime)method.Invoke(theTime, new object[] { 1 });
            }
            JobHelper.SetCronJob(() =>
            {
                DateTime trueDate = DateTime.Now + paramter.leadTime;
                string tableName = BuildTableName(trueDate);

                //自动创建数据库表
                GetGroupDbs(groupName).ForEach(aDb =>
                {
                    var entityType = ShardingHelper.MapTable(typeof(TEntity), tableName);
                    DbFactory.CreateTable(aDb.ConString, aDb.DbType, entityType);
                });

                //添加物理表
                AddPhysicTable<TEntity>(tableName, groupName);
            }, paramter.conExpression);

            return this;

            string BuildTableName(DateTime dateTime)
            {
                return $"{absTableName}_{dateTime.ToString(GetDateFormat(expandByDateMode))}";
            }

            List<(string ConString, DatabaseType DbType)> GetGroupDbs(string _groupName)
            {
                var q = from a in _physicDbs
                        join b in _physicDbGroups on a.GroupName equals b.GroupName
                        join c in _absDbs on b.AbsDbName equals c.Name
                        where b.GroupName == _groupName
                        select (a.ConString, c.DbType);

                return q.ToList();
            }
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
        private ConcurrentDictionary<string, Func<object, string>> _shardingRules { get; }
            = new ConcurrentDictionary<string, Func<object, string>>();
        private ConcurrentDictionary<string, ExpandByDateMode> _expandByDateMode { get; }
            = new ConcurrentDictionary<string, ExpandByDateMode>();
        private string GetTableKey(string absDbName, string absTableName)
        {
            return $"{absDbName}.{absTableName}";
        }
        private string GetDateFormat(ExpandByDateMode expandByDateMode)
        {
            return expandByDateMode switch
            {
                ExpandByDateMode.PerMinute => "yyyyMMddHHmm",
                ExpandByDateMode.PerHour => "yyyyMMddHH",
                ExpandByDateMode.PerDay => "yyyyMMdd",
                ExpandByDateMode.PerMonth => "yyyyMM",
                ExpandByDateMode.PerYear => "yyyy",
                _ => throw new Exception("ExpandByDateMode无效")
            };
        }
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

        public IConfigInit SetEntityAssembly(params string[] entityAssemblyNames)
        {
            ShardingConfig.AssemblyNames = entityAssemblyNames;

            return this;
        }

        #endregion
    }
}
