using Dynamitey;
using EFCore.Sharding.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static List<string> FindTablesByTime(IQueryable queryable, List<string> tables, string absTable, string shardingField)
        {
            var visitor = new FindTablesByTimeVisitor(tables, absTable, shardingField, queryable.ElementType);
            visitor.Visit(queryable.Expression);

            return visitor.GetResTables();
        }

        class FindTablesByTimeVisitor : ExpressionVisitor
        {
            private readonly List<string> _allTables;
            private readonly string _absTable;
            private Expression<Func<int, bool>> _where = x => true;
            private readonly string _shardingField;
            private readonly Type _absTableType;
            public FindTablesByTimeVisitor(List<string> allTables, string absTable, string shardingField, Type absTableType)
            {
                _allTables = allTables;
                _absTable = absTable;
                _shardingField = shardingField;
                _absTableType = absTableType;
            }

            public List<string> GetResTables()
            {
                return _allTables.Where(x => _where.Compile()(_allTables.IndexOf(x))).ToList();
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Where")
                {
                    var paramter = node.Arguments[0];
                    var body = ((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as BinaryExpression;
                    switch (body.NodeType)
                    {
                        case ExpressionType.GreaterThanOrEqual:
                            {
                                if (body.Left is MemberExpression member)
                                {
                                    if (member.Expression.Type == _absTableType && member.Member.Name == _shardingField)
                                    {
                                        if (body.Right is MemberExpression rightMember)
                                        {
                                            var time = (DateTime)Dynamic.InvokeGet((rightMember.Expression as ConstantExpression).Value, rightMember.Member.Name);

                                            string tableName = $"{_absTable}_{time:yyyyMMddHHmmss}";
                                            var newTables = _allTables.Concat(new string[] { tableName }).OrderBy(x => x).ToList();
                                            int index = newTables.IndexOf(tableName);
                                            if (index == newTables.Count - 1)
                                            {
                                                _where = _where.And(x => false);
                                            }
                                            else
                                            {
                                                _where = _where.And(x => x >= index - 1);
                                            }
                                        }
                                    }
                                }
                            }; break;
                        default: break;
                    }

                    string tmp = string.Empty;
                }

                return base.VisitMethodCall(node);
            }

            private Expression<Func<int, bool>> GetWhere(BinaryExpression binaryExpression)
            {
                Expression<Func<int, bool>> left = x => true;
                Expression<Func<int, bool>> right = x => true;

                //递归获取
                if (binaryExpression.Left is BinaryExpression)
                    left = GetWhere(binaryExpression.Left as BinaryExpression);
                if (binaryExpression.Right is BinaryExpression)
                    left = GetWhere(binaryExpression.Right as BinaryExpression);

                //组合
                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return left.And(right);
                }
                else if (binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    return left.Or(right);
                }

                //单个

                return x => true;
            }
        }
    }
}
