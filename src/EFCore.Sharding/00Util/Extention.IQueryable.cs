using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
#if EFCORE3
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
#endif
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Sharding.Util
{
    /// <summary>
    /// IQueryable"T"的拓展操作
    /// 作者：Coldairarrow
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
            var visitor = new OrderByVisitor();
            visitor.Visit(source.Expression);

            return visitor.OrderParam;
        }

        /// <summary>
        /// 切换数据源,保留原数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="targetSource">目标数据源</param>
        /// <returns></returns>
        public static IQueryable ChangeSource(this IQueryable source, IQueryable targetSource)
        {
            if (!(source is IQueryable && targetSource is IQueryable))
                throw new Exception("仅支持EF的IQueryable!");

            Dictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
            var oldQuery = source.GetObjQuery() as IQueryable;
            var newQuery = targetSource.GetObjQuery() as IQueryable;
            typeMap[oldQuery.ElementType] = newQuery.ElementType;
            var methods = GetMethods(source.Expression);
            Expression newExpression = newQuery.Expression;
            Expression oldExpression = oldQuery.Expression;

            while (true)
            {
                if (methods.Count == 0)
                    break;
                var theMethod = methods.Pop();
                string methodName = theMethod.Method.Name;
                if (theMethod.Method.Name == "AsNoTracking")
                    continue;

                var args = theMethod.Arguments.ToList();
                args[0] = newExpression;
                for (int i = 1; i < args.Count; i++)
                {
                    args[i] = new ArgumentVisitor(ChangeSource_BuildParamters(args[i], typeMap), typeMap).Visit(args[i]);
                }
                var genericArguments = theMethod.Method.GetGenericArguments().ToList();
                for (int i = 0; i < genericArguments.Count; i++)
                {
                    if (typeMap.ContainsKey(genericArguments[i]))
                        genericArguments[i] = typeMap[genericArguments[i]];
                }

                newExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    genericArguments.ToArray(),
                    args.ToArray());
                newQuery = newQuery.Provider.CreateQuery(newExpression);
                oldExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    theMethod.Method.GetGenericArguments(),
                    theMethod.Arguments.ToArray());

                oldQuery = oldQuery.Provider.CreateQuery(oldExpression);

                typeMap[oldQuery.ElementType] = newQuery.ElementType;
            }

            return targetSource.Provider.CreateQuery(newExpression);

            Stack<MethodCallExpression> GetMethods(Expression expression)
            {
                Stack<MethodCallExpression> resList = new Stack<MethodCallExpression>();

                Expression next = expression;
                while (true)
                {
                    if (next is MethodCallExpression methodCall)
                    {
                        resList.Push(methodCall);
                        next = methodCall.Arguments[0];
                    }
                    else
                    {
                        break;
                    }
                }

                return resList;
            }
        }

        /// <summary>
        /// 获取ObjectQuery
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable GetObjQuery(this IQueryable source)
        {
            GetObjQueryVisitor visitor = new GetObjQueryVisitor();
            visitor.Visit(source.Expression);

            return visitor.ObjQuery;
        }

        /// <summary>
        /// 转为SQL语句，包括参数
        /// </summary>
        /// <param name="query">查询原源</param>
        /// <returns></returns>
        public static (string sql, IReadOnlyDictionary<string, object> parameters) ToSql(this IQueryable query)
        {
            return IQueryableToSql.ToSql(query);
        }

#region 私有成员

        private static Dictionary<string, ParameterExpression> ChangeSource_BuildParamters(Expression expression, Dictionary<Type, Type> map)
        {
            Dictionary<string, ParameterExpression> res = new Dictionary<string, ParameterExpression>();
            GetParamtersVisitor visitor = new GetParamtersVisitor();
            visitor.Visit(expression);
            var paramters = visitor.Paramters;
            paramters.ForEach(aParamter =>
            {
                if (!res.ContainsKey(aParamter.Name))
                {
                    if (map.ContainsKey(aParamter.Type))
                    {
                        res[aParamter.Name] = Expression.Parameter(map[aParamter.Type], aParamter.Name);
                    }
                    else
                    {
                        res[aParamter.Name] = aParamter;
                    }
                }
            });

            return res;
        }
        private static Expression ChangeSource_VisitParameter(ParameterExpression node, Dictionary<string, ParameterExpression> paramters)
        {
            var newNode = node;
            if (paramters.ContainsKey(node.Name))
                newNode = paramters[node.Name];

            return newNode;
        }
        private static Expression ChangeSource_VisitMember(MemberExpression node, Dictionary<string, ParameterExpression> paramters)
        {
            MemberExpression newNode = node;
            if (node.Expression is ParameterExpression oldParamter)
            {
                if (paramters.ContainsKey(oldParamter.Name))
                {
                    var newParamter = paramters[oldParamter.Name];
                    newNode = Expression.MakeMemberAccess(newParamter, newParamter.Type.GetMember(node.Member.Name).Single());
                }
            }

            return newNode;
        }
        private static Expression ChangeSource_VisitLambda<T>(Expression<T> node, Dictionary<string, ParameterExpression> paramters, Dictionary<Type, Type> typeMap)
        {
            var lambdaVisitor = new LambdaVisitor(paramters, typeMap);
            var newLambdaBody = lambdaVisitor.Visit(node.Body);
            var theParamters = node.Parameters.Select(x => paramters[x.Name]).ToArray();
            var lambda = Expression.Lambda(newLambdaBody, theParamters);

            return lambda;
        }
        private static Expression ChangeSource_VisitMethodCall(MethodCallExpression node, Dictionary<Type, Type> typeMap, Dictionary<string, ParameterExpression> paramters = null)
        {
            var theMethod = node;
            var args = theMethod.Arguments.ToList();
            for (int i = 0; i < args.Count; i++)
            {
                args[i] = new ArgumentVisitor(paramters ?? ChangeSource_BuildParamters(args[i], typeMap), typeMap).Visit(args[i]);
            }
            var genericArguments = theMethod.Method.GetGenericArguments().ToList();
            for (int i = 0; i < genericArguments.Count; i++)
            {
                if (typeMap.ContainsKey(genericArguments[i]))
                    genericArguments[i] = typeMap[genericArguments[i]];
            }
            return Expression.Call(
                node.Method.DeclaringType,
                node.Method.Name,
                genericArguments.ToArray(),
                args.ToArray());
        }

#endregion

#region 自定义类

        class StatisVisitor : ExpressionVisitor
        {
            public StatisVisitor(ParameterExpression newParamter)
            {
                _newParamter = newParamter;
            }
            ParameterExpression _newParamter { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _newParamter;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return Expression.MakeMemberAccess(_newParamter, _newParamter.Type.GetMember(node.Member.Name).Single());
            }
        }

        class ArgumentVisitor : ExpressionVisitor
        {
            public ArgumentVisitor(
                Dictionary<string, ParameterExpression> paramters,
                Dictionary<Type, Type> typeMap)
            {
                _paramters = paramters;
                _typeMap = typeMap;
            }
            Dictionary<string, ParameterExpression> _paramters { get; }
            Dictionary<Type, Type> _typeMap { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ChangeSource_VisitParameter(node, _paramters);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return ChangeSource_VisitMember(node, _paramters);
            }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var newParamters = ChangeSource_BuildParamters(node, _typeMap);
                return ChangeSource_VisitLambda(node, newParamters, _typeMap);
            }
        }
        class LambdaVisitor : ExpressionVisitor
        {
            public LambdaVisitor(
                Dictionary<string, ParameterExpression> paramters,
                Dictionary<Type, Type> typeMap)
            {
                _paramters = paramters;
                _typeMap = typeMap;
            }
            Dictionary<string, ParameterExpression> _paramters { get; }
            Dictionary<Type, Type> _typeMap { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ChangeSource_VisitParameter(node, _paramters);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return ChangeSource_VisitMember(node, _paramters);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                string paramterName = node.Arguments[0] is ParameterExpression oldParamter ? oldParamter.Name : "";
                if (!_paramters.ContainsKey(paramterName))
                    return base.VisitMethodCall(node);

                return ChangeSource_VisitMethodCall(node, _typeMap, _paramters);
            }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return ChangeSource_VisitLambda(node, _paramters, _typeMap);
            }
        }

        class GetParamtersVisitor : ExpressionVisitor
        {
            public List<ParameterExpression> Paramters { get; set; } = new List<ParameterExpression>();
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (!Paramters.Contains(node))
                    Paramters.Add(node);
                return base.VisitParameter(node);
            }
        }

        class GetObjQueryVisitor : ExpressionVisitor
        {
            public IQueryable ObjQuery { get; set; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is IQueryable)
                    ObjQuery = node.Value as IQueryable;

                return base.VisitConstant(node);
            }
        }

        class OrderByVisitor : ExpressionVisitor
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
        /// 删除OrderBy表达式
        /// </summary>
        public class RemoveOrderByVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Enumerable) && node.Method.DeclaringType != typeof(Queryable))
                    return base.VisitMethodCall(node);

                if (node.Method.Name != "OrderBy" && node.Method.Name != "OrderByDescending" && node.Method.Name != "ThenBy" && node.Method.Name != "ThenByDescending")
                    return base.VisitMethodCall(node);

                //eliminate the method call from the expression tree by returning the object of the call.
                return base.Visit(node.Arguments[0]);
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

        public class ConstantVisitor : ExpressionVisitor
        {
            public ConstantVisitor(ConstantExpression oldConstant, ConstantExpression newConstant)
            {
                _oldConstant = oldConstant;
                _newConstant = newConstant;
            }
            private ConstantExpression _oldConstant { get; }
            private ConstantExpression _newConstant { get; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node == _oldConstant)
                    return _newConstant;

                return node;
            }
        }

#if EFCORE3
        static class IQueryableToSql
        {
            public static (string sql, IReadOnlyDictionary<string, object> parameters) ToSql(IQueryable query)
            {
                var enumerator = query.Provider.Execute<IEnumerable>(query.Expression).GetEnumerator();
                var queryContext = enumerator.GetGetFieldValue("_relationalQueryContext") as RelationalQueryContext;
                var relationalCommandCache = enumerator.GetGetFieldValue("_relationalCommandCache");
                var selectExpression = relationalCommandCache.GetGetFieldValue("_selectExpression") as SelectExpression;
                var factory = relationalCommandCache.GetGetFieldValue("_querySqlGeneratorFactory") as IQuerySqlGeneratorFactory;

                var sqlGenerator = factory.Create();
                var command = sqlGenerator.GetCommand(selectExpression);

                return (command.CommandText, queryContext.ParameterValues);
            }
        }
#endif

#if EFCORE2

        static class IQueryableToSql
        {
            private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
            private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
            private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");
            private static readonly FieldInfo queryContextFactoryField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryContextFactory");
            private static readonly FieldInfo loggerField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_logger");
            private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
            private static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

            public static (string sql, IReadOnlyDictionary<string, object> parameters) ToSql(IQueryable query)
            {
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
            }
        }

#endif

#endregion
    }
}
