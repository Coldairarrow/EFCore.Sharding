#if EFCORE3
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections;
#endif

#if EFCORE6
using Microsoft.EntityFrameworkCore;
#endif

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            var visitor = new SkipVisitor();
            visitor.Visit(source.Expression);

            return visitor.SkipCount;
        }

        /// <summary>
        /// 获取Take数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int? GetTakeCount(this IQueryable source)
        {
            var visitor = new TakeVisitor();
            visitor.Visit(source.Expression);

            return visitor.TakeCount;
        }

        /// <summary>
        /// 获取排序参数
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static (string sortColumn, string sortType) GetOrderBy(this IQueryable source)
        {
            var visitor = new GetOrderByVisitor();
            visitor.Visit(source.Expression);

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
            ReplaceQueryableVisitor replaceQueryableVisitor = new ReplaceQueryableVisitor(newSource);
            var newExpre = replaceQueryableVisitor.Visit(source.Expression);

            return newSource.Provider.CreateQuery(newExpre);
        }

        /// <summary>
        /// 转为SQL语句，包括参数
        /// </summary>
        /// <param name="query">查询原源</param>
        /// <returns></returns>
        public static (string sql, IReadOnlyDictionary<string, object> parameters) ToSql(this IQueryable query)
        {
#if EFCORE6
            var cmd = query.CreateDbCommand();
            Dictionary<string, object> paramters = new Dictionary<string, object>();
            foreach (DbParameter aCmd in cmd.Parameters)
            {
                paramters.Add(aCmd.ParameterName, aCmd.Value);
            }

            return (cmd.CommandText, paramters);
#endif
#if EFCORE3
            var enumerator = query.Provider.Execute<IEnumerable>(query.Expression).GetEnumerator();
            var queryContext = enumerator.GetGetFieldValue("_relationalQueryContext") as RelationalQueryContext;
            var relationalCommandCache = enumerator.GetGetFieldValue("_relationalCommandCache");
            var selectExpression = relationalCommandCache.GetGetFieldValue("_selectExpression") as SelectExpression;
            var factory = relationalCommandCache.GetGetFieldValue("_querySqlGeneratorFactory") as IQuerySqlGeneratorFactory;

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            return (command.CommandText, queryContext.ParameterValues);
#endif
        }

        #region 自定义类
#if !EFCORE6
        class ReplaceQueryableVisitor : ExpressionVisitor
        {
            private readonly IQueryable _newQuery;
            public ReplaceQueryableVisitor(IQueryable newQuery)
            {
                _newQuery = newQuery;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is IQueryable)
                {
                    return Expression.Constant(_newQuery);
                }

                return base.VisitConstant(node);
            }
        }
#endif
#if EFCORE6
        class ReplaceQueryableVisitor : ExpressionVisitor
        {
            private readonly QueryRootExpression _queryRootExpression;
            public ReplaceQueryableVisitor(IQueryable newQuery)
            {
                var visitor = new GetQueryRootVisitor();
                visitor.Visit(newQuery.Expression);
                _queryRootExpression = visitor.QueryRootExpression;
            }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is QueryRootExpression)
                {
                    return _queryRootExpression;
                }

                return base.VisitExtension(node);
            }
        }
        class GetQueryRootVisitor : ExpressionVisitor
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
#endif
        class GetOrderByVisitor : ExpressionVisitor
        {
            public (string sortColumn, string sortType) OrderParam { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending")
                {
                    string sortColumn = (((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression).Member.Name;
                    string sortType = node.Method.Name == "OrderBy" ? "asc" : "desc";
                    OrderParam = (sortColumn, sortType);
                }
                return base.VisitMethodCall(node);
            }
        }

        class SkipVisitor : ExpressionVisitor
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

        class TakeVisitor : ExpressionVisitor
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
                if (node.Method.Name == "Skip")
                    return base.Visit(node.Arguments[0]);

                return node;
            }
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        public class RemoveTakeVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Take")
                    return base.Visit(node.Arguments[0]);

                return node;
            }
        }

        #endregion
    }
}

