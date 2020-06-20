using Dynamitey;
using EFCore.Sharding.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace EFCore.Sharding
{
    internal static class ShardingHelper
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

        /// <summary>
        /// 通过时间筛选分表
        /// </summary>
        /// <param name="queryable">查询源</param>
        /// <param name="tables">所有分表名</param>
        /// <param name="absTable">抽象表名</param>
        /// <param name="shardingField">分表字段</param>
        /// <returns></returns>
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
                if (node.Method.Name == "Where"
                    && node.Arguments[1] is UnaryExpression unaryExpression
                    && unaryExpression.Operand is LambdaExpression lambdaExpression
                    && lambdaExpression.Body is BinaryExpression binaryExpression
                    )
                {
                    var newWhere = GetWhere(binaryExpression);

                    _where = _where.And(newWhere);
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
                    right = GetWhere(binaryExpression.Right as BinaryExpression);

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
                else
                {
                    bool paramterAtLeft;
                    DateTime? value = null;

                    if (IsParamter(binaryExpression.Left) && IsConstant(binaryExpression.Right))
                    {
                        paramterAtLeft = true;
                        value = GetTime(binaryExpression.Right);
                    }
                    else if (IsConstant(binaryExpression.Left) && IsParamter(binaryExpression.Right))
                    {
                        paramterAtLeft = false;
                        value = GetTime(binaryExpression.Left);
                    }
                    else
                        return x => true;

                    string op = binaryExpression.NodeType switch
                    {
                        ExpressionType.GreaterThan => paramterAtLeft ? ">" : "<",
                        ExpressionType.GreaterThanOrEqual => paramterAtLeft ? ">=" : "<=",
                        ExpressionType.LessThan => paramterAtLeft ? "<" : ">",
                        ExpressionType.LessThanOrEqual => paramterAtLeft ? "<=" : ">=",
                        ExpressionType.Equal => "==",
                        ExpressionType.NotEqual => "!=",
                        _ => null
                    };

                    if (op == null || value == null)
                        return x => true;

                    string tableName = $"{_absTable}_{value.Value:yyyyMMddHHmmss}";
                    var newTables = _allTables.Concat(new string[] { tableName }).OrderBy(x => x).ToList();
                    int index = newTables.IndexOf(tableName);

                    if (binaryExpression.NodeType == ExpressionType.GreaterThan
                        || binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual
                        || binaryExpression.NodeType == ExpressionType.Equal
                        || binaryExpression.NodeType == ExpressionType.NotEqual
                        )
                    {
                        index = index - 1;
                    }

                    var newWhere = DynamicExpressionParser.ParseLambda<int, bool>(
                        ParsingConfig.Default, false, $@"it {op} @0", index);

                    return newWhere;
                }

                bool IsParamter(Expression expression)
                {
                    return expression is MemberExpression member
                        && member.Expression.Type == _absTableType
                        && member.Member.Name == _shardingField;
                }

                bool IsConstant(Expression expression)
                {
                    return expression is ConstantExpression
                        || (expression is MemberExpression member && member.Expression is ConstantExpression);
                }

                DateTime? GetTime(Expression expression)
                {
                    if (expression is ConstantExpression constant1)
                    {
                        return (DateTime?)constant1.Value;
                    }
                    else if (expression is MemberExpression member && member.Expression is ConstantExpression constant2)
                    {
                        return (DateTime?)Dynamic.InvokeGet(constant2.Value, member.Member.Name);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
