#if EFCORE3
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections;
#elif EFCORE2
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
#endif
#if EFCORE5
using Microsoft.EntityFrameworkCore;
#endif

using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
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
#if EFCORE5
            var yourQuery = query;//Query Code
            var visitor = new ChangeVarsToLiteralsVisitor();
            var changedExpression = visitor.Visit(yourQuery.Expression);
            var newQuery = query.Provider.CreateQuery(changedExpression);
            return (newQuery.ToQueryString(), null);
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
#elif EFCORE2
            TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
            FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
            FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");
            FieldInfo queryContextFactoryField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryContextFactory");
            FieldInfo loggerField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_logger");
            FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
            PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

            var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
            var queryContextFactory = (IQueryContextFactory)queryContextFactoryField.GetValue(queryCompiler);
            var logger = (Microsoft.EntityFrameworkCore.Diagnostics.IDiagnosticsLogger<DbLoggerCategory.Query>)loggerField.GetValue(queryCompiler);
            var queryContext = queryContextFactory.Create();
            var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
            var newQueryExpression = modelGenerator.ExtractParameters(logger, query.Expression, queryContext);
            var queryModel = modelGenerator.ParseQuery(newQueryExpression);
            var database = (IDatabase)DataBaseField.GetValue(queryCompiler);
            var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
            var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();

            modelVisitor.GetType()
                .GetMethod("CreateQueryExecutor")
                .MakeGenericMethod(query.ElementType)
                .Invoke(modelVisitor, new object[] { queryModel });

            var command = modelVisitor.Queries.First().CreateDefaultQuerySqlGenerator()
                .GenerateSql(queryContext.ParameterValues);

            return (command.CommandText, queryContext.ParameterValues);
#endif
        }

        #region 自定义类

        public class ChangeVarsToLiteralsVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                // Recurse down to see if we can simplify...
                var expression = Visit(memberExpression.Expression);

                // If we've ended up with a constant, and it's a property or a field,
                // we can simplify ourselves to a constant
                if (expression is ConstantExpression)
                {
                    object container = ((ConstantExpression)expression).Value;
                    var member = memberExpression.Member;

                    if (member is FieldInfo)
                    {
                        object value = ((FieldInfo)member).GetValue(container);
                        return Expression.Constant(value);
                    }

                    if (member is PropertyInfo)
                    {
                        object value = ((PropertyInfo)member).GetValue(container, null);
                        return Expression.Constant(value);
                    }
                }

                return base.VisitMember(memberExpression);
            }
        }

#if !EFCORE5
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
#if EFCORE5
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

