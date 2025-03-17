using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.Sharding
{
    /// <summary>
    /// IQueryable"T"的拓展操作
    /// </summary>
    internal static partial class Extention
    {
        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveSkip<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveSkip();
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveSkip(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveSkipVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveTake<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveTake();
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveTake(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveTakeVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 获取Skip数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int? GetSkipCount(this IQueryable source)
        {
            SkipVisitor visitor = new();
            _ = visitor.Visit(source.Expression);

            return visitor.SkipCount;
        }

        /// <summary>
        /// 获取Take数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int? GetTakeCount(this IQueryable source)
        {
            TakeVisitor visitor = new();
            _ = visitor.Visit(source.Expression);

            return visitor.TakeCount;
        }

        /// <summary>
        /// 获取排序参数
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static (string sortColumn, string sortType) GetOrderBy(this IQueryable source)
        {
            GetOrderByVisitor visitor = new();
            _ = visitor.Visit(source.Expression);

            return visitor.OrderParam;
        }

        /// <summary>
        /// 切换数据源,保留原数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="newSource">新数据源</param>
        /// <returns></returns>
        public static IQueryable ReplaceQueryable(this IQueryable source, IQueryable newSource)
        {
            ReplaceQueryableVisitor replaceQueryableVisitor = new(newSource);
            Expression newExpre = replaceQueryableVisitor.Visit(source.Expression);

            return newSource.Provider.CreateQuery(newExpre);
        }

        /// <summary>
        /// 转为SQL语句，包括参数
        /// </summary>
        /// <param name="query">查询原源</param>
        /// <returns></returns>
        public static (string sql, IReadOnlyDictionary<string, object> parameters) ToSql(this IQueryable query)
        {
            DbCommand cmd = query.CreateDbCommand();
            Dictionary<string, object> paramters = [];
            foreach (DbParameter aCmd in cmd.Parameters)
            {
                paramters.Add(aCmd.ParameterName, aCmd.Value);
            }

            return (cmd.CommandText, paramters);
        }

        #region 自定义类
        private class ReplaceQueryableVisitor : ExpressionVisitor
        {
            private readonly QueryRootExpression _queryRootExpression;
            public ReplaceQueryableVisitor(IQueryable newQuery)
            {
                GetQueryRootVisitor visitor = new();
                _ = visitor.Visit(newQuery.Expression);
                _queryRootExpression = visitor.QueryRootExpression;
            }

            protected override Expression VisitExtension(Expression node)
            {
                return node is QueryRootExpression ? _queryRootExpression : base.VisitExtension(node);
            }
        }
        private class GetQueryRootVisitor : ExpressionVisitor
        {
            public QueryRootExpression QueryRootExpression { get; set; }
            protected override Expression VisitExtension(Expression node)
            {
                if (node is QueryRootExpression expression)
                {
                    QueryRootExpression = expression;
                }

                return base.VisitExtension(node);
            }
        }
        private class GetOrderByVisitor : ExpressionVisitor
        {
            public (string sortColumn, string sortType) OrderParam { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name is "OrderBy" or "OrderByDescending")
                {
                    string sortColumn = (((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression).Member.Name;
                    string sortType = node.Method.Name == "OrderBy" ? "asc" : "desc";
                    OrderParam = (sortColumn, sortType);
                }
                return base.VisitMethodCall(node);
            }
        }

        private class SkipVisitor : ExpressionVisitor
        {
            public int? SkipCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Skip")
                {
                    SkipCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
        }

        private class TakeVisitor : ExpressionVisitor
        {
            public int? TakeCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Take")
                {
                    TakeCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        public class RemoveSkipVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Method.Name == "Skip" ? base.Visit(node.Arguments[0]) : node;
            }
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        public class RemoveTakeVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Method.Name == "Take" ? base.Visit(node.Arguments[0]) : node;
            }
        }

        #endregion
    }
}

