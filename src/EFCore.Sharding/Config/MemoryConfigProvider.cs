using EFCore.Sharding.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EFCore.Sharding
{
    internal class MemoryConfigProvider : IConfigInit, IConfigProvider
    {
        #region 外部接口

        public IConfigInit SetEntityAssemblyPath(params string[] entityAssemblyPaths)
        {
            ShardingConfig.AssemblyPaths.AddRange(entityAssemblyPaths);

            return this;
        }

        public IConfigInit SetEntityAssembly(params string[] entityAssemblyNames)
        {
            ShardingConfig.AssemblyNames.AddRange(entityAssemblyNames);

            return this;
        }

        public IConfigInit UseDatabase<TDbAccessor>(string conString, DatabaseType dbType) where TDbAccessor : class, IDbAccessor
        {
            if (ShardingConfig.ServiceDescriptors != null)
            {
                ShardingConfig.ServiceDescriptors.AddScoped(_ =>
                {
                    ILoggerFactory loggerFactory = _.GetService<ILoggerFactory>();

                    IDbAccessor repository = DbFactory.GetDbAccessor(conString, dbType, loggerFactory);
                    if (ShardingConfig.LogicDelete)
                        repository = new LogicDeleteDbAccessor(repository);

                    if (typeof(TDbAccessor) == typeof(IDbAccessor))
                        return (TDbAccessor)repository;
                    else
                        return repository.ActLike<TDbAccessor>();
                });
            }

            return this;
        }

        public IConfigInit UseDatabase(string conString, DatabaseType dbType)
        {
            return UseDatabase<IDbAccessor>(conString, dbType);
        }

        public IConfigInit UseLogicDelete(string keyField = "Id", string deletedField = "Deleted")
        {
            ShardingConfig.LogicDelete = true;
            ShardingConfig.DeletedField = deletedField;
            ShardingConfig.KeyField = keyField;

            return this;
        }

        public DatabaseType GetAbsDbType(string absDbName)
        {
            var theDb = _absDbs.Where(x => x.Name == absDbName).FirstOrDefault();
            if (theDb.IsNullOrEmpty())
                throw new Exception("缺少抽象数据库");

            return theDb.DbType;
        }

        public List<(string tableName, string conString, DatabaseType dbType)> GetReadTables(string absTableName, string absDbName, IQueryable source)
        {
            var rule = GetShardingRule(absDbName, absTableName);
            var allTables = GetTargetTables(absTableName, ReadWriteType.Read, absDbName);
            var allTableNames = allTables.Select(x => x.tableName).ToList();
            var findTables = ShardingHelper.FilterTable(source, allTableNames, rule);
            allTables = allTables.Where(x => findTables.Contains(x.tableName)).ToList();
#if DEBUG
            Console.WriteLine($"查询分表:{string.Join(",", findTables)}");
#endif
            return allTables;
        }

        public List<(string tableName, string conString, DatabaseType dbType)> GetAllWriteTables<T>(string absDbName)
        {
            var absTableName = AnnotationHelper.GetDbTableName(typeof(T));

            return GetTargetTables(absTableName, ReadWriteType.Write, absDbName, null);
        }

        public (string tableName, string conString, DatabaseType dbType) GetTheWriteTable<T>(object obj, string absDbName)
        {
            var absTableName = AnnotationHelper.GetDbTableName(typeof(T));

            return GetTargetTables(absTableName, ReadWriteType.Write, absDbName, obj).Single();
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
            var absTableName = AnnotationHelper.GetDbTableName(absEntityType);

            _lock.EnterReadLock();
            bool exists = _physicTables.Any(x =>
                x.AbsTableName == absTableName
                && x.GroupName == groupName
                && x.PhysicTableName == physicTableName);
            _lock.ExitReadLock();
            if (exists)
                return this;

            _lock.EnterWriteLock();
            _physicTables.Add(new PhysicTable
            {
                AbsTableName = absTableName,
                GroupName = groupName,
                PhysicTableName = physicTableName
            });
            _lock.ExitWriteLock();

            var physicEntityType = ShardingHelper.MapTable(absEntityType, physicTableName);
            DbModelFactory.AddEntityType(physicTableName, physicEntityType);

            return this;
        }

        public IConfigInit SetHashModShardingRule<TEntity>(
            string shardingField,
            int mod,
            bool createTable = true,
            string groupName = "BaseDbGroup",
            string absDbName = "BaseDb")
        {
            CheckRule<TEntity>(absDbName, ShardingType.HashMod, shardingField);

            ShardingRule rule = new ShardingRule
            {
                AbsDb = absDbName,
                AbsTableType = typeof(TEntity),
                ShardingField = shardingField,
                Mod = mod,
                ShardingType = ShardingType.HashMod
            };
            _shardingRules.Add(rule);

            //建表
            var groupDbs = GetGroupDbs(groupName);
            if (createTable)
            {
                for (int i = 0; i < mod; i++)
                {
                    string tableName = $"{rule.AbsTable}_{i}";
                    AddPhysicTable<TEntity>(tableName, groupName);

                    groupDbs.ForEach(aDb =>
                    {
                        var entityType = ShardingHelper.MapTable(typeof(TEntity), tableName);
                        DbFactory.CreateTable(aDb.ConString, aDb.DbType, entityType);
                    });
                }
            }

            return this;
        }

        public IConfigInit SetDateShardingRule<TEntity>(string shardingField, string absDbName = "BaseDb")
        {
            CheckRule<TEntity>(absDbName, ShardingType.Date, shardingField);

            _shardingRules.Add(new ShardingRule
            {
                AbsDb = absDbName,
                AbsTableType = typeof(TEntity),
                ShardingField = shardingField,
                ShardingType = ShardingType.Date
            });

            return this;
        }

        public IConfigInit AutoExpandByDate<TEntity>(
            ExpandByDateMode expandByDateMode,
            params (DateTime startTime, DateTime endTime, string groupName)[] ranges)
        {
            var aRange = ranges.FirstOrDefault();
            string absDbName = _physicDbGroups.Where(x => x.GroupName == aRange.groupName).FirstOrDefault()?.AbsDbName;
            if (absDbName.IsNullOrEmpty())
                throw new Exception("缺少抽象数据库与物理数据库组信息");
            string absTableName = AnnotationHelper.GetDbTableName(typeof(TEntity));

            var shardingRule = GetShardingRule(absDbName, absTableName);
            if (shardingRule == null)
                throw new Exception($"请设置分表规则 抽象表:{absTableName}");

            //分表规则赋值
            shardingRule.ExpandByDateMode = expandByDateMode;

            (string conExpression, TimeSpan leadTime) paramter =
               expandByDateMode switch
               {
                   ExpandByDateMode.PerMinute => ("30 * * * * ? *", TimeSpan.FromSeconds(60)),
                   ExpandByDateMode.PerHour => ("0 30 * * * ? *", TimeSpan.FromMinutes(60)),
                   ExpandByDateMode.PerDay => ("0 0 23 * * ? *", TimeSpan.FromHours(2)),
                   ExpandByDateMode.PerMonth => ("0 0 0 L * ? *", TimeSpan.FromDays(2)),
                   ExpandByDateMode.PerYear => ("0 0 0 L 12 ? *", TimeSpan.FromDays(2)),
                   _ => throw new Exception("expandByDateMode参数无效")
               };

            JobHelper.SetCronJob(() =>
            {
                DateTime trueDate = DateTime.Now + paramter.leadTime;
                string tableName = shardingRule.GetTableNameByField(trueDate);
                string groupName = GetTheGroup(trueDate);
                //自动创建数据库表
                GetGroupDbs(groupName).ForEach(aDb =>
                {
                    var entityType = ShardingHelper.MapTable(typeof(TEntity), tableName);
                    DbFactory.CreateTable(aDb.ConString, aDb.DbType, entityType);
                });

                //添加物理表
                AddPhysicTable<TEntity>(tableName, groupName);
            }, paramter.conExpression);

            //确保之前的表已存在
            var theTime = ranges.Min(x => x.startTime);

            var key = expandByDateMode.ToString().Replace("Per", "");
            var method = theTime.GetType().GetMethod($"Add{key}s");

            DateTime endTime = (DateTime)method.Invoke(DateTime.Now, new object[] { 1 }) + paramter.leadTime;

            while (theTime <= endTime)
            {
                string tableName = shardingRule.GetTableNameByField(theTime);
                string groupName = GetTheGroup(theTime);
                AddPhysicTable<TEntity>(tableName, groupName);
                //建表
                //自动创建数据库表
                GetGroupDbs(groupName).ForEach(aDb =>
                {
                    var entityType = ShardingHelper.MapTable(typeof(TEntity), tableName);
                    DbFactory.CreateTable(aDb.ConString, aDb.DbType, entityType);
                });

                theTime = (DateTime)method.Invoke(theTime, new object[] { 1 });
            }

            return this;

            string GetTheGroup(DateTime time)
            {
                return ranges.Where(x => time >= x.startTime && time < x.endTime).FirstOrDefault().groupName;
            }
        }

        #endregion

        #region 私有成员

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly SynchronizedCollection<AbsDb> _absDbs
            = new SynchronizedCollection<AbsDb>();
        private readonly SynchronizedCollection<PhysicDbGroup> _physicDbGroups
            = new SynchronizedCollection<PhysicDbGroup>();
        private readonly SynchronizedCollection<PhysicDb> _physicDbs
            = new SynchronizedCollection<PhysicDb>();
        private readonly SynchronizedCollection<PhysicTable> _physicTables
            = new SynchronizedCollection<PhysicTable>();
        private readonly SynchronizedCollection<ShardingRule> _shardingRules
            = new SynchronizedCollection<ShardingRule>();

        private List<(string tableName, string conString, DatabaseType dbType)>
            GetTargetTables(string absTableName, ReadWriteType opType, string absDbName, object obj = null)
        {
            _lock.EnterReadLock();
            var rule = GetShardingRule(absDbName, absTableName);

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

            _lock.ExitReadLock();

            //若为写操作则只获取特定表
            if (!obj.IsNullOrEmpty())
            {
                string tableName = rule.GetTableNameByEntity(obj);
                groupList = groupList.Where(x => x.PhysicTableName == tableName).ToList();
            }

            //数据库组中数据库负载均衡
            var resList = groupList.Select(x =>
            {
                var dbs = _physicDbs
                    .Where(y => y.GroupName == x.GroupName && y.OpType.HasFlag(opType))
                    .ToList();
                var theDb = RandomHelper.Next(dbs);

                return (x.PhysicTableName, theDb.ConString, x.DbType);
            }).ToList();

            return resList;
        }
        private ShardingRule GetShardingRule(string absDb, string absTable)
        {
            return _shardingRules.Where(x => x.AbsDb == absDb && x.AbsTable == absTable).FirstOrDefault();
        }
        private void CheckRule<TEntity>(string absDb, ShardingType shardingType, string shardingField)
        {
            string absTable = AnnotationHelper.GetDbTableName(typeof(TEntity));
            if (_shardingRules.Any(x => x.AbsDb == absDb && x.AbsTable == absTable))
                throw new Exception($"{absTable}已存在分表规则!");

            Type fieldType = typeof(TEntity).GetProperty(shardingField)?.PropertyType;
            if (fieldType == null)
                throw new Exception($"不存在分表字段:{shardingField}");
            if (fieldType.IsNullable())
                throw new Exception($"分表字段:{shardingField}不能为可空类型");

            if (shardingType == ShardingType.Date)
            {
                if (fieldType != typeof(DateTime))
                {
                    throw new Exception($"分表字段:{shardingField}类型必须为DateTime");
                }
            }
        }
        private List<(string ConString, DatabaseType DbType)> GetGroupDbs(string groupName)
        {
            var q = from a in _physicDbs
                    join b in _physicDbGroups on a.GroupName equals b.GroupName
                    join c in _absDbs on b.AbsDbName equals c.Name
                    where b.GroupName == groupName
                    select (a.ConString, c.DbType);

            return q.ToList();
        }

        #endregion
    }
}
