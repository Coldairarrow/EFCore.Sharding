using Dynamitey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace EFCore.Sharding
{
    internal static class ShardingHelper
    {
        public static List<string> FilterTable(IQueryable queryable, List<string> tableSuffixs, ShardingRule rule)
        {
            FilterTableVisitor visitor = rule.ShardingType switch
            {
                ShardingType.HashMod => new FilterTableByHashModVisitor(tableSuffixs, rule),
                ShardingType.Date => new FilterTableByDateVisitor(tableSuffixs, rule),
                _ => throw new Exception("ShardingType无效")
            };

            visitor.Visit(queryable.Expression);

            return visitor.GetResTables();
        }

        private abstract class FilterTableVisitor : ExpressionVisitor
        {
            protected readonly List<string> _allTableSuffixs;
            protected readonly ShardingRule _rule;
            public FilterTableVisitor(List<string> allTableSuffixs, ShardingRule rule)
            {
                _allTableSuffixs = allTableSuffixs;
                _rule = rule;
            }
            protected bool IsParamter(Expression expression)
            {
                return expression is MemberExpression member
                    && member.Expression.Type == _rule.EntityType
                    && member.Member.Name == _rule.ShardingField;
            }
            protected bool IsConstant(Expression expression)
            {
                return expression is ConstantExpression
                    || (expression is MemberExpression member && member.Expression is ConstantExpression);
            }
            protected object GetFieldValue(Expression expression)
            {
                if (expression is ConstantExpression constant1)
                {
                    return constant1.Value;
                }
                else if (expression is MemberExpression member && member.Expression is ConstantExpression constant2)
                {
                    return Dynamic.InvokeGet(constant2.Value, member.Member.Name);
                }
                else
                {
                    return null;
                }
            }
            public abstract List<string> GetResTables();
        }
        private class FilterTableByDateVisitor : FilterTableVisitor
        {
            private Expression<Func<int, bool>> _where = x => true;
            public FilterTableByDateVisitor(List<string> allTableSuffixs, ShardingRule rule)
                : base(allTableSuffixs, rule)
            {
            }
            public override List<string> GetResTables()
            {
                return _allTableSuffixs.Where(x => _where.Compile()(_allTableSuffixs.IndexOf(x))).ToList();
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
                        value = (DateTime?)GetFieldValue(binaryExpression.Right);
                    }
                    else if (IsConstant(binaryExpression.Left) && IsParamter(binaryExpression.Right))
                    {
                        paramterAtLeft = false;
                        value = (DateTime?)GetFieldValue(binaryExpression.Left);
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

                    string tableSuffix = value.Value.ToString("yyyyMMddHHmmss");
                    var newTableSuffixs = _allTableSuffixs.Concat(new string[] { tableSuffix }).OrderBy(x => x).ToList();
                    int index = newTableSuffixs.IndexOf(tableSuffix);

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
            }
        }

        private class FilterTableByHashModVisitor : FilterTableVisitor
        {
            private Expression<Func<string, bool>> _where = x => true;
            public FilterTableByHashModVisitor(List<string> allTables, ShardingRule rule)
                : base(allTables, rule)
            {
            }
            public override List<string> GetResTables()
            {
                return _allTableSuffixs.Where(_where.Compile()).ToList();
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
            private Expression<Func<string, bool>> GetWhere(BinaryExpression binaryExpression)
            {
                Expression<Func<string, bool>> left = x => true;
                Expression<Func<string, bool>> right = x => true;

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
                else if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    object value = null;

                    if (IsParamter(binaryExpression.Left) && IsConstant(binaryExpression.Right))
                    {
                        value = GetFieldValue(binaryExpression.Right);
                    }
                    else if (IsConstant(binaryExpression.Left) && IsParamter(binaryExpression.Right))
                    {
                        value = GetFieldValue(binaryExpression.Left);
                    }
                    else
                        return x => true;

                    if (value == null)
                        return x => true;

                    string suffix = _rule.GetTableSuffixByField(value);

                    var newWhere = DynamicExpressionParser.ParseLambda<string, bool>(
                        ParsingConfig.Default, false, $@"it == @0", suffix);

                    return newWhere;
                }
                else
                    return x => true;
            }
        }
    }
}
