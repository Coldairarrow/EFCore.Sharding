using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly SynchronizedCollection<DataSource> _dataSources
            = [];
        private readonly SynchronizedCollection<ShardingRule> _shardingRules
            = [];
        private readonly SynchronizedCollection<PhysicTable> _physicTables
            = [];
        private List<(string suffix, string conString, DatabaseType dbType)>
            GetTargetTables<TEntity>(ReadWriteType opType, object obj = null)
        {
            Type entityType = typeof(TEntity);
            ShardingRule rule = _shardingRules.Where(x => x.EntityType == entityType).FirstOrDefault();

            //获取数据库组
            List<PhysicTable> tables = _physicTables.Where(x => x.EntityType == entityType).ToList();

            //若为写操作则只获取特定表
            if (obj != null)
            {
                string tableSuffix = rule.GetTableSuffixByEntity(obj);
                tables = tables.Where(x => x.Suffix == tableSuffix).ToList();
            }

            //数据库组中数据库负载均衡
            List<(string Suffix, string connectionString, DatabaseType DbType)> resList = tables.Select(x =>
            {
                DataSource theSource = _dataSources.Where(y => y.Name == x.DataSourceName).FirstOrDefault();

                List<(string connectionString, ReadWriteType readWriteType)> dbs = theSource.Dbs.Where(y => y.readWriteType.HasFlag(opType)).ToList();
                (string connectionString, ReadWriteType readWriteType) = RandomHelper.Next(dbs);

                return (x.Suffix, connectionString, theSource.DbType);
            }).ToList();

            return resList;
        }
        private void CheckRule<TEntity>(ShardingType shardingType, string shardingField)
        {
            if (_shardingRules.Any(x => x.EntityType == typeof(TEntity)))
            {
                throw new Exception($"{typeof(TEntity).Name}已存在分表规则!");
            }

            Type fieldType = typeof(TEntity).GetProperty(shardingField)?.PropertyType;
            if (fieldType == null)
            {
                throw new Exception($"不存在分表字段:{shardingField}");
            }

            if (fieldType.IsNullable())
            {
                throw new Exception($"分表字段:{shardingField}不能为可空类型");
            }

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
            Type entityType = typeof(TEntity);

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
        private void CreateTable<TEntity>(IServiceProvider serviceProvider, string sourceName, string suffix)
        {
            DataSource theSource = _dataSources.Where(x => x.Name == sourceName).FirstOrDefault();
            theSource.Dbs.ForEach(aDb =>
            {
                serviceProvider.GetService<DbFactory>().CreateTable(aDb.connectionString, theSource.DbType, typeof(TEntity), suffix);
            });
        }
        private List<(string suffix, string conString, DatabaseType dbType)> FilterTable<T>(
            List<(string suffix, string conString, DatabaseType dbType)> allTables, IQueryable<T> source)
        {
            Type entityType = typeof(T);
            string absTable = AnnotationHelper.GetDbTableName(source.ElementType);
            ShardingRule rule = _shardingRules.Where(x => x.EntityType == entityType).Single();
            List<string> allTableSuffixs = allTables.Select(x => x.suffix).ToList();
            List<string> findSuffixs = ShardingHelper.FilterTable(source, allTableSuffixs, rule);
            allTables = allTables.Where(x => findSuffixs.Contains(x.suffix)).ToList();
#if DEBUG
            Console.WriteLine($"访问分表:{string.Join(",", findSuffixs.Select(x => $"{absTable}_{x}"))}");
#endif
            return allTables;
        }
        private void AddShardingTable(string absTableName, string fullTableName)
        {
            if (!ExistsShardingTables.ContainsKey(absTableName))
            {
                ExistsShardingTables.Add(absTableName, []);
            }
            ExistsShardingTables[absTableName].Add(fullTableName);
        }

        #endregion

        #region 配置提供

        public List<(string suffix, string conString, DatabaseType dbType)> GetWriteTables<T>(IQueryable<T> source = null)
        {
            List<(string suffix, string conString, DatabaseType dbType)> tables = GetTargetTables<T>(ReadWriteType.Write, null);
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
            List<(string suffix, string conString, DatabaseType dbType)> allTables = GetTargetTables<T>(ReadWriteType.Read);

            return FilterTable(allTables, source);
        }
        public DatabaseType FindADbType()
        {
            return _dataSources.FirstOrDefault().DbType;
        }
        public readonly Dictionary<string, List<string>> ExistsShardingTables
            = [];

        #endregion

        #region 配置构建

        public IShardingBuilder SetEntityAssemblies(params Assembly[] assemblies)
        {
            EFCoreShardingOptions.EntityAssemblies = assemblies;

            return this;
        }
        public IShardingBuilder SetCommandTimeout(int timeout)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.CommandTimeout = timeout;
            });

            return this;
        }
        public IShardingBuilder AddEntityTypeBuilderFilter(Action<EntityTypeBuilder> filter)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.EntityTypeBuilderFilter += filter;
            });

            return this;
        }
        public IShardingBuilder MigrationsWithoutForeignKey()
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.MigrationsWithoutForeignKey = true;
            });

            return this;
        }
        public IShardingBuilder CreateShardingTableOnStarting(bool enable)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.CreateShardingTableOnStarting = enable;
            });

            return this;
        }
        public IShardingBuilder EnableShardingMigration(bool enable)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.EnableShardingMigration = enable;
            });

            return this;
        }
        public IShardingBuilder EnableComments(bool enable)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.EnableComments = enable;
            });

            return this;
        }
        public IShardingBuilder UseLogicDelete(string keyField = "Id", string deletedField = "Deleted")
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.LogicDelete = true;
                x.KeyField = keyField;
                x.DeletedField = deletedField;
            });

            return this;
        }
        public IShardingBuilder SetMinCommandElapsedMilliseconds(int minCommandElapsedMilliseconds)
        {
            _ = _services.Configure<EFCoreShardingOptions>(x =>
            {
                x.MinCommandElapsedMilliseconds = minCommandElapsedMilliseconds;
            });

            return this;
        }
        public IShardingBuilder UseDatabase(string conString, DatabaseType dbType, string entityNamespace = null, Action<EFCoreShardingOptions> optionsBuilder = null)
        {
            return UseDatabase<IDbAccessor>(conString, dbType, entityNamespace, optionsBuilder);
        }
        public IShardingBuilder UseDatabase<TDbAccessor>(string conString, DatabaseType dbType, string entityNamespace, Action<EFCoreShardingOptions> optionsBuilder = null) where TDbAccessor : class, IDbAccessor
        {
            string optionName = typeof(TDbAccessor).FullName;
            _ = _services.AddOptions<EFCoreShardingOptions>(optionName);

            if (optionsBuilder != null)
            {
                _ = _services.Configure(optionName, optionsBuilder);
            }

            _ = _services.AddScoped(serviceProvider =>
            {
                IDbFactory dbFactory = serviceProvider.GetService<IDbFactory>();
                EFCoreShardingOptions options = serviceProvider.GetService<IOptionsMonitor<EFCoreShardingOptions>>().BuildOption(optionName);
                IDbAccessor db = dbFactory.GetDbAccessor(new DbContextParamters
                {
                    ConnectionString = conString,
                    DbType = dbType,
                    EntityNamespace = entityNamespace
                }, optionName);
                if (options.LogicDelete)
                {
                    db = new LogicDeleteDbAccessor(db, options);
                }

                return typeof(TDbAccessor) == typeof(IDbAccessor) ? (TDbAccessor)db : db.ActLike<TDbAccessor>();
            });

            return this;
        }
        public IShardingBuilder UseDatabase((string connectionString, ReadWriteType readWriteType)[] dbs, DatabaseType dbType, string entityNamespace = null, Action<EFCoreShardingOptions> optionsBuilder = null)
        {
            return UseDatabase<IDbAccessor>(dbs, dbType, entityNamespace, optionsBuilder);
        }
        public IShardingBuilder UseDatabase<TDbAccessor>((string connectionString, ReadWriteType readWriteType)[] dbs, DatabaseType dbType, string entityNamespace, Action<EFCoreShardingOptions> optionsBuilder = null) where TDbAccessor : class, IDbAccessor
        {
            string optionName = typeof(TDbAccessor).FullName;
            _ = _services.AddOptions<EFCoreShardingOptions>(optionName);

            if (optionsBuilder != null)
            {
                _ = _services.Configure(optionName, optionsBuilder);
            }

            if (!(dbs.Any(x => x.readWriteType.HasFlag(ReadWriteType.Read))
                && dbs.Any(x => x.readWriteType.HasFlag(ReadWriteType.Write))))
            {
                throw new Exception("dbs必须包含写库与读库");
            }

            _ = _services.AddScoped(serviceProvider =>
            {
                EFCoreShardingOptions options = serviceProvider.GetService<IOptionsMonitor<EFCoreShardingOptions>>().BuildOption(optionName);

                IDbAccessor db = new ReadWriteDbAccessor(
                    dbs,
                    dbType,
                    entityNamespace,
                    serviceProvider.GetService<IDbFactory>(),
                    options
                    );

                return typeof(TDbAccessor) == typeof(IDbAccessor) ? (TDbAccessor)db : db.ActLike<TDbAccessor>();
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

            ShardingRule shardingRule = new()
            {
                EntityType = typeof(TEntity),
                ExpandByDateMode = expandByDateMode,
                ShardingField = shardingField,
                ShardingType = ShardingType.Date
            };
            _shardingRules.Add(shardingRule);

            EFCoreShardingOptions.Bootstrapper += serviceProvider =>
            {
                EFCoreShardingOptions sharingOption = serviceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;

                (string conExpression, string startTimeFormat, Func<DateTime, DateTime> nextTime) paramter =
                    expandByDateMode switch
                    {
                        ExpandByDateMode.PerMinute => ("0 * * * * ? *", "yyyy/MM/dd HH:mm:00", x => x.AddMinutes(1)),
                        ExpandByDateMode.PerHour => ("0 0 * * * ? *", "yyyy/MM/dd HH:00:00", x => x.AddHours(1)),
                        ExpandByDateMode.PerDay => ("0 0 0 * * ? *", "yyyy/MM/dd 00:00:00", x => x.AddDays(1)),
                        ExpandByDateMode.PerMonth => ("0 0 0 1 * ? *", "yyyy/MM/01 00:00:00", x => x.AddMonths(1)),
                        ExpandByDateMode.PerYear => ("0 0 0 1 1 ? *", "yyyy/01/01 00:00:00", x => x.AddYears(1)),
                        _ => throw new Exception("expandByDateMode参数无效")
                    };

                //确保之前的表已存在
                DateTime theTime = ranges.Min(x => x.startTime);
                theTime = DateTime.Parse(theTime.ToString(paramter.startTimeFormat));

                DateTime endTime = paramter.nextTime(DateTime.Parse(DateTime.Now.ToString(paramter.startTimeFormat)));

                while (theTime <= endTime)
                {
                    string theSourceName = GetSourceName(theTime);
                    string suffix = shardingRule.GetTableSuffixByField(theTime);

                    string absTableName = AnnotationHelper.GetDbTableName(typeof(TEntity));
                    string fullTableName = $"{absTableName}_{suffix}";
                    AddShardingTable(absTableName, fullTableName);

                    //启动时建表
                    if (sharingOption.CreateShardingTableOnStarting)
                    {
                        CreateTable<TEntity>(serviceProvider, theSourceName, suffix);
                    }

                    AddPhysicTable<TEntity>(suffix, theSourceName);

                    theTime = paramter.nextTime(theTime);
                }

                //定时自动建表
                _ = JobHelper.SetCronJob(() =>
                {
                    DateTime trueDate = paramter.nextTime(DateTime.Parse(DateTime.Now.ToString(paramter.startTimeFormat)));
                    string theSourceName = GetSourceName(trueDate);
                    string suffix = shardingRule.GetTableSuffixByField(trueDate);
                    //添加物理表
                    CreateTable<TEntity>(serviceProvider, theSourceName, suffix);
                    AddPhysicTable<TEntity>(suffix, theSourceName);
                }, paramter.conExpression);

                string GetSourceName(DateTime time)
                {
                    return ranges
                        .Where(x => time >= DateTime.Parse(x.startTime.ToString(paramter.startTimeFormat))
                            && time < x.endTime)
                        .FirstOrDefault()
                        .sourceName;
                }
            };

            return this;
        }
        public IShardingBuilder SetHashModSharding<TEntity>(string shardingField, int mod, string sourceName = "DefaultSource")
        {
            return SetHashModSharding<TEntity>(shardingField, mod, (0, mod, sourceName));
        }
        public IShardingBuilder SetHashModSharding<TEntity>(string shardingField, int mod, params (int start, int end, string sourceName)[] ranges)
        {
            CheckRule<TEntity>(ShardingType.HashMod, shardingField);

            ShardingRule rule = new()
            {
                EntityType = typeof(TEntity),
                ShardingField = shardingField,
                Mod = mod,
                ShardingType = ShardingType.HashMod
            };
            _shardingRules.Add(rule);

            EFCoreShardingOptions.Bootstrapper += serviceProvider =>
            {
                EFCoreShardingOptions sharingOption = serviceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;

                //建表
                for (int i = 0; i < mod; i++)
                {
                    string sourceName = ranges.Where(x => i >= x.start && i < x.end).FirstOrDefault().sourceName;

                    string absTableName = AnnotationHelper.GetDbTableName(typeof(TEntity));
                    string fullTableName = $"{absTableName}_{i}";
                    AddShardingTable(absTableName, fullTableName);

                    //启动时建表
                    if (sharingOption.CreateShardingTableOnStarting)
                    {
                        CreateTable<TEntity>(serviceProvider, sourceName, i.ToString());
                    }

                    AddPhysicTable<TEntity>(i.ToString(), sourceName);
                }
            };

            return this;
        }

        #endregion
    }
}
