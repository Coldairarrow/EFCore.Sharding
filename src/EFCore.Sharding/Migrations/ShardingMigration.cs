#if EFCORE3
using EFCore.Sharding.Config;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EFCore.Sharding
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<挂起>")]
    internal class ShardingMigration : MigrationsModelDiffer
    {
        private readonly Dictionary<string, List<string>> _existsShardingTables
            = new Dictionary<string, List<string>>();
        public ShardingMigration(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotations,
            IChangeDetector changeDetector,
            IUpdateAdapterFactory updateAdapterFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies
            )
        : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
        {
            _existsShardingTables = Cache.ServiceProvider.GetService<ShardingContainer>().ExistsShardingTables;
        }

        public override IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            List<MigrationOperation> resList = new List<MigrationOperation>();

            var shardingOption = Cache.ServiceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;
            var sourceOperations = base.GetDifferences(source, target).ToList();

            //忽略外键
            if (shardingOption.MigrationsWithoutForeignKey)
            {
                sourceOperations.RemoveAll(x => x is AddForeignKeyOperation || x is DropForeignKeyOperation);
                foreach (var operation in sourceOperations.OfType<CreateTableOperation>())
                {
                    operation.ForeignKeys?.Clear();
                }
            }

            if (shardingOption.EnableShardingMigration)
            {
                //分表
                resList.AddRange(sourceOperations.SelectMany(x => BuildShardingOperation(x)));
            }
            else
            {
                resList.AddRange(sourceOperations);
            }

            return resList;
        }

        private List<MigrationOperation> BuildShardingOperation(MigrationOperation sourceOperation)
        {
            //所有MigrationOperation定义
            //https://github.com/dotnet/efcore/tree/b970bf29a46521f40862a01db9e276e6448d3cb0/src/EFCore.Relational/Migrations/Operations
            //ColumnOperation仅替换Table
            //其余其余都是将Name和Table使用分表名替换

            List<MigrationOperation> resList = new List<MigrationOperation>();
            string absTableName = string.Empty;

            string name = sourceOperation.GetPropertyValue("Name") as string;
            string tableName = sourceOperation.GetPropertyValue("Table") as string;
            string pattern = string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            Func<KeyValuePair<string, List<string>>, bool> where = x =>
                _existsShardingTables.Any(x => Regex.IsMatch(name, BuildPattern(x.Key)));

            if (!tableName.IsNullOrEmpty())
            {
                absTableName = tableName;
            }
            else if (!name.IsNullOrEmpty() && _existsShardingTables.Any(x => where(x)))
            {
                absTableName = _existsShardingTables.Where(x => where(x)).FirstOrDefault().Key;
            }

            //分表
            if (!absTableName.IsNullOrEmpty() && _existsShardingTables.ContainsKey(absTableName))
            {
                var shardings = _existsShardingTables[absTableName];
                shardings.ForEach(aSharding =>
                {
                    var clone = sourceOperation.DeepClone();
                    resList.Add(clone);
                    ReplaceName(clone, absTableName, aSharding);
                });
            }
            //不分表
            else
            {
                resList.Add(sourceOperation);
            }

            return resList;

            string BuildPattern(string absTableName)
            {
                return string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            }
        }

        private void ReplaceName(MigrationOperation theOperation, string sourceName, string targetName)
        {
            string name = theOperation.GetPropertyValue("Name") as string;
            string tableName = theOperation.GetPropertyValue("Table") as string;
            if (!tableName.IsNullOrEmpty())
            {
                theOperation.SetPropertyValue("Table", targetName);
            }
            if (!name.IsNullOrEmpty() && !(theOperation is ColumnOperation))
            {
                string[] patterns = new string[] { $"^()({sourceName})()$", $"^()({sourceName})(_.*?)$", $"^(.*?_)({sourceName})(_.*?)$", $"^(.*?_)({sourceName})()$" };
                foreach (var aPattern in patterns)
                {
                    if (Regex.IsMatch(name, aPattern))
                    {
                        var newName = new Regex(aPattern).Replace(name, "${1}" + targetName + "$3");
                        theOperation.SetPropertyValue("Name", newName);
                        break;
                    }
                }
            }
            Func<PropertyInfo, bool> propertyWhere = x =>
                x.PropertyType.IsGenericType
                && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                && typeof(MigrationOperation).IsAssignableFrom(x.PropertyType.GetGenericArguments()[0]);
            //其它
            theOperation.GetType().GetProperties()
                .Where(x => x.Name != "Name"
                    && x.Name != "Table"
                    && x.PropertyType != typeof(object)
                    && (typeof(MigrationOperation).IsAssignableFrom(x.PropertyType) || propertyWhere(x))
                    )
                .ToList()
                .ForEach(aProperty =>
                {
                    var propertyValue = aProperty.GetValue(theOperation);
                    if (propertyValue is MigrationOperation propertyOperation)
                    {
                        ReplaceName(propertyOperation, sourceName, targetName);
                    }
                    else if (propertyWhere(aProperty))
                    {
                        foreach (var aValue in (IEnumerable)propertyValue)
                        {
                            ReplaceName((MigrationOperation)aValue, sourceName, targetName);
                        }
                    }
                });
        }
    }
}

#endif