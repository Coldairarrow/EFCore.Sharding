using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Sharding
{
    internal class ShardingContainer : IShardingConfig, IShardingBuilder
    {
        #region 构造函数

        private readonly IServiceCollection _services;
        public ShardingContainer(IServiceCollection services)
        {
            _services = services;
        }

        #endregion

        #region 私有成员

        private List<string> AssemblyNames = new List<string>();
        private List<string> AssemblyPaths
            = new List<string>() { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };
        private List<Type> _allEntityTypes;
        private object _entityLock = new object();
        private readonly SynchronizedCollection<DataSource> _dataSources
            = new SynchronizedCollection<DataSource>();
        private readonly SynchronizedCollection<ShardingRule> _shardingRules
            = new SynchronizedCollection<ShardingRule>();
        private readonly SynchronizedCollection<PhysicTable> _physicTables
            = new SynchronizedCollection<PhysicTable>();
        private List<(string suffix, string conString, DatabaseType dbType)>
            GetTargetTables<TEntity>(ReadWriteType opType, object obj = null)
        {
            var entityType = typeof(TEntity);
            var rule = _shardingRules.Where(x => x.EntityType == entityType).FirstOrDefault();

            //获取数据库组
            var tables = _physicTables.Where(x => x.EntityType == entityType).ToList();

            //若为写操作则只获取特定表
            if (obj != null)
            {
                string tableSuffix = rule.GetTableSuffixByEntity(obj);
                tables = tables.Where(x => x.Suffix == tableSuffix).ToList();
            }

            //数据库组中数据库负载均衡
            var resList = tables.Select(x =>
            {
                var theSource = _dataSources.Where(y => y.Name == x.DataSourceName).FirstOrDefault();

                var dbs = theSource.Dbs.Where(y => y.readWriteType.HasFlag(opType)).ToList();
                var theDb = RandomHelper.Next(dbs);

                return (x.Suffix, theDb.connectionString, theSource.DbType);
            }).ToList();

            return resList;
        }
        private void CheckRule<TEntity>(ShardingType shardingType, string shardingField)
        {
            if (_shardingRules.Any(x => x.EntityType == typeof(TEntity)))
                throw new Exception($"{typeof(TEntity).Name}已存在分表规则!");

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
        private void AddPhysicTable<TEntity>(string suffix, string sourceName)
        {
            var entityType = typeof(TEntity);

            if (!_physicTables.Any(x => x.EntityType == entityType && x.Suffix == suffix && x.DataSourceName == sourceName))
            {
                _physicTables.Add(new PhysicTable
                {
                    DataSourceName = sourceName,
                    EntityType = entityType,
                    Suffix = suffix
                });
            }
        }
        private void CreateTable<TEntity>(string sourceName, string suffix)
        {
            var theSource = _dataSources.Where(x => x.Name == sourceName).FirstOrDefault();
            theSource.Dbs.ForEach(aDb =>
            {
                DbFactory.CreateTable(aDb.connectionString, theSource.DbType, typeof(TEntity), suffix);
            });
        }
        private List<(string suffix, string conString, DatabaseType dbType)> FilterTable<T>(
            List<(string suffix, string conString, DatabaseType dbType)> allTables, IQueryable<T> source)
        {
            var entityType = typeof(T);
            string absTable = AnnotationHelper.GetDbTableName(source.ElementType);
            var rule = _shardingRules.Where(x => x.EntityType == entityType).Single();
            var allTableSuffixs = allTables.Select(x => x.suffix).ToList();
            var findSuffixs = ShardingHelper.FilterTable(source, allTableSuffixs, rule);
            allTables = allTables.Where(x => findSuffixs.Contains(x.suffix)).ToList();
#if DEBUG
            Console.WriteLine($"访问分表:{string.Join(",", findSuffixs.Select(x => $"{absTable}_{x}"))}");
#endif
            return allTables;
        }

        #endregion

        #region 配置提供

        public List<(string suffix, string conString, DatabaseType dbType)> GetWriteTables<T>(IQueryable<T> source = null)
        {
            var tables = GetTargetTables<T>(ReadWriteType.Write, null);
            if (source != null)
            {
                tables = FilterTable(tables, source);
            }

            return tables;
        }
        public (string suffix, string conString, DatabaseType dbType) GetTheWriteTable<T>(T obj)
        {
            return GetTargetTables<T>(ReadWriteType.Write, obj).Single();
        }
        public List<(string suffix, string conString, DatabaseType dbType)> GetReadTables<T>(IQueryable<T> source)
        {
            var allTables = GetTargetTables<T>(ReadWriteType.Read);

            return FilterTable(allTables, source);
        }
        public bool LogicDelete { get; set; } = false;
        public string KeyField { get; set; } = "Id";
        public string DeletedField { get; set; } = "Deleted";
        public DatabaseType FindADbType()
        {
            return _dataSources.FirstOrDefault().DbType;
        }
        public List<Type> AllEntityTypes
        {
            get
            {
                if (_allEntityTypes == null)
                {
                    lock (_entityLock)
                    {
                        if (_allEntityTypes == null)
                        {
                            _allEntityTypes = new List<Type>();

                            Expression<Func<string, bool>> where = x => true;
                            where = where.And(x =>
                                  !x.Contains("System.")
                                  && !x.Contains("Microsoft."));
                            if (AssemblyNames.Count > 0)
                            {
                                Expression<Func<string, bool>> tmpWhere = x => false;
                                AssemblyNames.ToList().ForEach(aAssembly =>
                                {
                                    tmpWhere = tmpWhere.Or(x => x.Contains(aAssembly));
                                });

                                where = where.And(tmpWhere);
                            }

                            AssemblyPaths.SelectMany(x => Directory.GetFiles(x, "*.dll"))
                                .Where(x => where.Compile()(new FileInfo(x).Name))
                                .Distinct()
                                .Select(x =>
                                {
                                    try
                                    {
                                        return Assembly.LoadFrom(x);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                })
                                .Where(x => x != null && !x.IsDynamic)
                                .ForEach(aAssembly =>
                                {
                                    try
                                    {
                                        _allEntityTypes.AddRange(aAssembly.GetTypes());
                                    }

                                    catch
                                    {

                                    }
                                });
                        }
                    }
                }

                return _allEntityTypes;
            }
        }

        #endregion

        #region 配置构建

        public IShardingBuilder SetEntityAssemblyPath(params string[] entityAssemblyPaths)
        {
            AssemblyPaths.AddRange(entityAssemblyPaths);

            return this;
        }
        public IShardingBuilder SetEntityAssembly(params string[] entityAssemblyNames)
        {
            AssemblyNames.AddRange(entityAssemblyNames);

            return this;
        }
        public IShardingBuilder UseLogicDelete(string keyField = "Id", string deletedField = "Deleted")
        {
            LogicDelete = true;
            KeyField = keyField;
            DeletedField = deletedField;

            return this;
        }
        public IShardingBuilder UseDatabase(string conString, DatabaseType dbType, string entityNamespace = null)
        {
            return UseDatabase<IDbAccessor>(conString, dbType, entityNamespace);
        }
        public IShardingBuilder UseDatabase<TDbAccessor>(string conString, DatabaseType dbType, string entityNamespace) where TDbAccessor : class, IDbAccessor
        {
            _services.AddScoped(_ =>
            {
                var dbFactory = _.GetService<IDbFactory>();
                var config = _.GetService<IShardingConfig>();
                IDbAccessor db = dbFactory.GetDbAccessor(conString, dbType, entityNamespace);
                if (config.LogicDelete)
                    db = new LogicDeleteDbAccessor(db, config);

                if (typeof(TDbAccessor) == typeof(IDbAccessor))
                    return (TDbAccessor)db;
                else
                    return db.ActLike<TDbAccessor>();
            });

            return this;
        }
        public IShardingBuilder UseDatabase((string connectionString, ReadWriteType readWriteType)[] dbs, DatabaseType dbType, string entityNamespace = null)
        {
            return UseDatabase<IDbAccessor>(dbs, dbType, entityNamespace);
        }
        public IShardingBuilder UseDatabase<TDbAccessor>((string connectionString, ReadWriteType readWriteType)[] dbs, DatabaseType dbType, string entityNamespace) where TDbAccessor : class, IDbAccessor
        {
            if (!(dbs.Any(x => x.readWriteType.HasFlag(ReadWriteType.Read))
                && dbs.Any(x => x.readWriteType.HasFlag(ReadWriteType.Write))))
                throw new Exception("dbs必须包含写库与读库");

            _services.AddScoped(_ =>
            {
                IDbAccessor db = new ReadWriteDbAccessor(
                    dbs,
                    dbType,
                    entityNamespace,
                    _.GetService<IDbFactory>(),
                    _.GetService<IShardingConfig>()
                    );

                if (typeof(TDbAccessor) == typeof(IDbAccessor))
                    return (TDbAccessor)db;
                else
                    return db.ActLike<TDbAccessor>();
            });

            return this;
        }
        public IShardingBuilder AddDataSource(string connectionString, ReadWriteType readWriteType, DatabaseType dbType, string sourceName = "DefaultSource")
        {
            return AddDataSource(new (string, ReadWriteType)[] { (connectionString, readWriteType) }, dbType, sourceName);
        }
        public IShardingBuilder AddDataSource((string connectionString, ReadWriteType readWriteType)[] dbs, DatabaseType dbType, string sourceName = "DefaultSource")
        {
            _dataSources.Add(new DataSource
            {
                Dbs = dbs,
                DbType = dbType,
                Name = sourceName
            });

            return this;
        }
        public IShardingBuilder SetDateSharding<TEntity>(string shardingField, ExpandByDateMode expandByDateMode, DateTime startTime, string sourceName = "DefaultSource")
        {
            return SetDateSharding<TEntity>(shardingField, expandByDateMode, (startTime, DateTime.MaxValue, sourceName));
        }
        public IShardingBuilder SetDateSharding<TEntity>(string shardingField, ExpandByDateMode expandByDateMode, params (DateTime startTime, DateTime endTime, string sourceName)[] ranges)
        {
            CheckRule<TEntity>(ShardingType.Date, shardingField);

            var shardingRule = new ShardingRule
            {
                EntityType = typeof(TEntity),
                ExpandByDateMode = expandByDateMode,
                ShardingField = shardingField,
                ShardingType = ShardingType.Date
            };
            _shardingRules.Add(shardingRule);

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

            //确保之前的表已存在
            var theTime = ranges.Min(x => x.startTime);

            var key = expandByDateMode.ToString().Replace("Per", "");
            var method = theTime.GetType().GetMethod($"Add{key}s");

            DateTime endTime = (DateTime)method.Invoke(DateTime.Now, new object[] { 1 }) + paramter.leadTime;

            while (theTime <= endTime)
            {
                var theSourceName = GetSourceName(theTime);
                string suffix = shardingRule.GetTableSuffixByField(theTime);
                CreateTable<TEntity>(theSourceName, suffix);
                AddPhysicTable<TEntity>(suffix, theSourceName);

                theTime = (DateTime)method.Invoke(theTime, new object[] { 1 });
            }

            //定时自动建表
            JobHelper.SetCronJob(() =>
            {
                DateTime trueDate = DateTime.Now + paramter.leadTime;
                var theSourceName = GetSourceName(trueDate);
                string suffix = shardingRule.GetTableSuffixByField(trueDate);
                //添加物理表
                CreateTable<TEntity>(theSourceName, suffix);
                AddPhysicTable<TEntity>(suffix, theSourceName);
            }, paramter.conExpression);

            return this;

            string GetSourceName(DateTime time)
            {
                return ranges.Where(x => time >= x.startTime && time < x.endTime).FirstOrDefault().sourceName;
            }
        }
        public IShardingBuilder SetHashModSharding<TEntity>(string shardingField, int mod, string sourceName = "DefaultSource")
        {
            return SetHashModSharding<TEntity>(shardingField, mod, (0, mod, sourceName));
        }
        public IShardingBuilder SetHashModSharding<TEntity>(string shardingField, int mod, params (int start, int end, string sourceName)[] ranges)
        {
            CheckRule<TEntity>(ShardingType.HashMod, shardingField);

            ShardingRule rule = new ShardingRule
            {
                EntityType = typeof(TEntity),
                ShardingField = shardingField,
                Mod = mod,
                ShardingType = ShardingType.HashMod
            };
            _shardingRules.Add(rule);

            //建表
            for (int i = 0; i < mod; i++)
            {
                var sourceName = ranges.Where(x => i >= x.start && i < x.end).FirstOrDefault().sourceName;
                CreateTable<TEntity>(sourceName, i.ToString());
                AddPhysicTable<TEntity>(i.ToString(), sourceName);
            }

            return this;
        }

        #endregion
    }
}
