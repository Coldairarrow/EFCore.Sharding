using System;
using System.Linq.Expressions;

namespace EFCore.Sharding
{
    internal static partial class Extention
    {
        #region 拓展And和Or方法

        /// <summary>
        /// 连接表达式与运算
        /// </summary>
        /// <typeparam name="T">参数</typeparam>
        /// <param name="one">原表达式</param>
        /// <param name="another">新的表达式</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> one, Expression<Func<T, bool>> another)
        {
            //创建新参数
            ParameterExpression newParameter = Expression.Parameter(typeof(T), "parameter");

            ParameterReplaceVisitor parameterReplacer = new(newParameter);
            Expression left = parameterReplacer.Visit(one.Body);
            Expression right = parameterReplacer.Visit(another.Body);
            BinaryExpression body = Expression.And(left, right);

            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 连接表达式或运算
        /// </summary>
        /// <typeparam name="T">参数</typeparam>
        /// <param name="one">原表达式</param>
        /// <param name="another">新表达式</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> one, Expression<Func<T, bool>> another)
        {
            //创建新参数
            ParameterExpression newParameter = Expression.Parameter(typeof(T), "parameter");

            ParameterReplaceVisitor parameterReplacer = new(newParameter);
            Expression left = parameterReplacer.Visit(one.Body);
            Expression right = parameterReplacer.Visit(another.Body);
            BinaryExpression body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        #endregion
    }

    /// <summary>
    /// 继承ExpressionVisitor类，实现参数替换统一
    /// </summary>
    internal class ParameterReplaceVisitor : ExpressionVisitor
    {
        public ParameterReplaceVisitor(ParameterExpression paramExpr)
        {
            _parameter = paramExpr;
        }

        //新的表达式参数
        private ParameterExpression _parameter { get; set; }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return p.Type == _parameter.Type ? _parameter : (Expression)p;
        }
    }
}
