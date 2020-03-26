using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// IShardingQueryable
    /// </summary>
    /// <typeparam name="T">逻辑表泛型</typeparam>
    public interface IShardingQueryable<T> where T : class, new()
    {
        /// <summary>
        /// 筛选
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        IShardingQueryable<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 动态筛选
        /// </summary>
        /// <param name="predicate">动态表达式</param>
        /// <param name="values">参数</param>
        /// <returns></returns>
        IShardingQueryable<T> Where(string predicate, params object[] values);

        /// <summary>
        /// SKip
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        IShardingQueryable<T> Skip(int count);

        /// <summary>
        /// Take
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        IShardingQueryable<T> Take(int count);

        /// <summary>
        /// 顺序排序
        /// </summary>
        /// <typeparam name="TKey">返回类型</typeparam>
        /// <param name="keySelector">表达式</param>
        /// <returns></returns>
        IShardingQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// 倒序排序
        /// </summary>
        /// <typeparam name="TKey">返回类型</typeparam>
        /// <param name="keySelector">表达式</param>
        /// <returns></returns>
        IShardingQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// 动态排序
        /// </summary>
        /// <param name="ordering">动态表达式</param>
        /// <param name="values">参数</param>
        /// <returns></returns>
        IShardingQueryable<T> OrderBy(string ordering, params object[] values);

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// 异步获取数量
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<T> ToList();

        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ToListAsync();

        /// <summary>
        /// 获取第一个,若不存在则返回默认值
        /// </summary>
        /// <returns></returns>
        T FirstOrDefault();

        /// <summary>
        /// 获取第一个,若不存在则返回默认值
        /// </summary>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync();

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        bool Any(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 异步判断是否存在
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 计算最大值
        /// </summary>
        /// <typeparam name="TResult">数据类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        TResult Max<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 异步计算最大值
        /// </summary>
        /// <typeparam name="TResult">数据类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 计算最小值
        /// </summary>
        /// <typeparam name="TResult">数据类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        TResult Min<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 异步计算最小值
        /// </summary>
        /// <typeparam name="TResult">数据类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double Average(Expression<Func<T, int>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double> AverageAsync(Expression<Func<T, int>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double? Average(Expression<Func<T, int?>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double?> AverageAsync(Expression<Func<T, int?>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        float Average(Expression<Func<T, float>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<float> AverageAsync(Expression<Func<T, float>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        float? Average(Expression<Func<T, float?>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<float?> AverageAsync(Expression<Func<T, float?>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double Average(Expression<Func<T, long>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double> AverageAsync(Expression<Func<T, long>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double? Average(Expression<Func<T, long?>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double?> AverageAsync(Expression<Func<T, long?>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double Average(Expression<Func<T, double>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double> AverageAsync(Expression<Func<T, double>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double? Average(Expression<Func<T, double?>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double?> AverageAsync(Expression<Func<T, double?>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        decimal Average(Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        decimal? Average(Expression<Func<T, decimal?>> selector);

        /// <summary>
        /// 异步计算平均值
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        decimal Sum(Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<decimal> SumAsync(Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        decimal? Sum(Expression<Func<T, decimal?>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double Sum(Expression<Func<T, double>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double> SumAsync(Expression<Func<T, double>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        double? Sum(Expression<Func<T, double?>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<double?> SumAsync(Expression<Func<T, double?>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        float Sum(Expression<Func<T, float>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<float> SumAsync(Expression<Func<T, float>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        float? Sum(Expression<Func<T, float?>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<float?> SumAsync(Expression<Func<T, float?>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        int Sum(Expression<Func<T, int>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<int> SumAsync(Expression<Func<T, int>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        int? Sum(Expression<Func<T, int?>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<int?> SumAsync(Expression<Func<T, int?>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        long Sum(Expression<Func<T, long>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<long> SumAsync(Expression<Func<T, long>> selector);

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        long? Sum(Expression<Func<T, long?>> selector);

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <param name="selector">表达式</param>
        /// <returns></returns>
        Task<long?> SumAsync(Expression<Func<T, long?>> selector);
    }
}
