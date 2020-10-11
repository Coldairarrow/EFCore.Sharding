using EFCore.Sharding.Config;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EFCore.Sharding.Migrations
{
    internal static class MigrationHelper
    {
        public static void Generate(
            MigrationOperation operation,
            MigrationCommandListBuilder builder,
            ISqlGenerationHelper sqlGenerationHelper,
            List<MigrationCommand> addCmds
            )
        {
            var shardingOption = Cache.ServiceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;
            if(!shardingOption.EnableShardingMigration)
            {
                return;
            }

            addCmds.ForEach(aAddCmd =>
            {
                var shardingCmds = BuildShardingCmds(operation, aAddCmd.CommandText, sqlGenerationHelper);
                shardingCmds.ForEach(aShardingCmd =>
                {
                    builder.Append(aShardingCmd)
                        .EndCommand();
                });
            });
        }

        private static List<string> BuildShardingCmds(MigrationOperation operation, string sourceCmd, ISqlGenerationHelper sqlGenerationHelper)
        {
            //所有MigrationOperation定义
            //https://github.com/dotnet/efcore/tree/b970bf29a46521f40862a01db9e276e6448d3cb0/src/EFCore.Relational/Migrations/Operations
            //ColumnOperation仅替换Table
            //其余其余都是将Name和Table使用分表名替换
            Dictionary<string, List<string>> _existsShardingTables
                = Cache.ServiceProvider.GetService<ShardingContainer>().ExistsShardingTables;

            List<string> resList = new List<string>();
            string absTableName = string.Empty;

            string name = operation.GetPropertyValue("Name") as string;
            string tableName = operation.GetPropertyValue("Table") as string;
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
                shardings.ForEach(aShardingTable =>
                {
                    string newCmd = sourceCmd;
                    GetReplaceGroups(operation, absTableName, aShardingTable).ForEach(aReplace =>
                    {
                        newCmd = newCmd.Replace(
                            sqlGenerationHelper.DelimitIdentifier(aReplace.sourceName),
                            sqlGenerationHelper.DelimitIdentifier(aReplace.targetName));
                    });
                    resList.Add(newCmd);
                });
            }

            return resList;

            string BuildPattern(string absTableName)
            {
                return string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            }
        }
        private static List<(string sourceName, string targetName)> GetReplaceGroups(
            MigrationOperation operation, string sourceTableName, string targetTableName)
        {
            List<(string sourceName, string targetName)> resList =
                new List<(string sourceName, string targetName)>
                {
                    (sourceTableName, targetTableName)
                };

            string name = operation.GetPropertyValue("Name") as string;
            if (!name.IsNullOrEmpty() && !(operation is ColumnOperation))
            {
                string[] patterns = new string[] { $"^()({sourceTableName})()$", $"^()({sourceTableName})(_.*?)$", $"^(.*?_)({sourceTableName})(_.*?)$", $"^(.*?_)({sourceTableName})()$" };
                foreach (var aPattern in patterns)
                {
                    if (Regex.IsMatch(name, aPattern))
                    {
                        var newName = new Regex(aPattern).Replace(name, "${1}" + targetTableName + "$3");
                        resList.Add((name, newName));
                        break;
                    }
                }
            }
            Func<PropertyInfo, bool> listPropertyWhere = x =>
                x.PropertyType.IsGenericType
                && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                && typeof(MigrationOperation).IsAssignableFrom(x.PropertyType.GetGenericArguments()[0]);
            //其它
            operation.GetType().GetProperties()
                .Where(x => x.Name != "Name"
                    && x.Name != "Table"
                    && x.PropertyType != typeof(object)
                    && (typeof(MigrationOperation).IsAssignableFrom(x.PropertyType) || listPropertyWhere(x))
                    )
                .ToList()
                .ForEach(aProperty =>
                {
                    var propertyValue = aProperty.GetValue(operation);
                    if (propertyValue is MigrationOperation propertyOperation)
                    {
                        resList.AddRange(GetReplaceGroups(propertyOperation, sourceTableName, targetTableName));
                    }
                    else if (listPropertyWhere(aProperty))
                    {
                        foreach (var aValue in (IEnumerable)propertyValue)
                        {
                            resList.AddRange(GetReplaceGroups((MigrationOperation)aValue, sourceTableName, targetTableName));
                        }
                    }
                });

            return resList;
        }
    }
}
