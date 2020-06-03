using EFCore.Sharding.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.Sharding
{
    public static class ShardingHelper
    {
        /// <summary>
        /// 映射物理表
        /// </summary>
        /// <param name="absTable">抽象表类型</param>
        /// <param name="targetTableName">目标物理表名</param>
        /// <returns></returns>
        public static Type MapTable(Type absTable, string targetTableName)
        {
            var config = TypeBuilderHelper.GetConfig(absTable);

            config.AssemblyName = "EFCore.Sharding";

            var theTableAttribute = config.Attributes
                .Where(x => x.Attribute == typeof(TableAttribute))
                .FirstOrDefault();
            if (theTableAttribute != null)
            {
                theTableAttribute.ConstructorArgs[0] = targetTableName;
            }

            config.FullName = $"EFCore.Sharding.{targetTableName}";

            return TypeBuilderHelper.BuildType(config);
        }

        public static List<string> FindTablesByTime(IQueryable queryable, List<string> tables, Func<DateTime, string> dateToTable)
        {
            var visitor = new FindTablesByTimeVisitor(tables, dateToTable);
            visitor.Visit(queryable.Expression);

            return visitor.ResTables;
        }

        class FindTablesByTimeVisitor : ExpressionVisitor
        {
            private readonly List<string> _allTables;
            private readonly Func<DateTime, string> _dateToTable;
            public FindTablesByTimeVisitor(List<string> allTables, Func<DateTime, string> dateToTable)
            {
                _allTables = allTables;
                _dateToTable = dateToTable;
            }

            public List<string> ResTables { get; } = new List<string>();

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Where")
                {
                    var paramter = node.Arguments[0];
                    var body = ((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body;

                    string tmp = string.Empty;
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}
