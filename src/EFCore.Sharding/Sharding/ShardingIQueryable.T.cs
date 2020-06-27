﻿using EFCore.Sharding.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class ShardingQueryable<T> : IShardingQueryable<T> where T : class, new()
    {
        #region 构造函数

        public ShardingQueryable(IQueryable<T> source, ShardingDbAccessor repository, string absDbName)
        {
            _source = source;
            _absTableName = AnnotationHelper.GetDbTableName(source.ElementType);
            _absDbName = absDbName;
            _repository = repository;
        }

        #endregion

        #region 私有成员

        ShardingDbAccessor _repository { get; }
        private string _absDbName { get; }
        private string _absTableName { get; }
        private IQueryable<T> _source { get; set; }
        private Type MapTable(string targetTableName)
        {
            return DbModelFactory.GetEntityType(targetTableName);
        }
        private async Task<List<TResult>> GetStatisDataAsync<TResult>(Func<IQueryable, Task<TResult>> access, IQueryable newSource = null)
        {
            newSource = newSource ?? _source;
            var tables = ShardingConfig.ConfigProvider.GetReadTables(_absTableName, _absDbName, _source);

            List<Task<TResult>> tasks = new List<Task<TResult>>();
            SynchronizedCollection<IDbAccessor> dbs = new SynchronizedCollection<IDbAccessor>();
            tasks = tables.Select(aTable =>
            {
                IDbAccessor db;
                if (_repository.OpenedTransaction)
                    db = _repository.GetMapDbAccessor(aTable.conString, aTable.dbType);
                else
                    db = DbFactory.GetDbAccessor(aTable.conString, aTable.dbType);

                dbs.Add(db);
                var targetTable = MapTable(aTable.tableName);
                var targetIQ = db.GetIQueryable(targetTable);
                var newQ = newSource.ChangeSource(targetIQ);

                return access(newQ);
            }).ToList();
            var res = (await Task.WhenAll(tasks)).ToList();

            if (!_repository.OpenedTransaction)
                dbs.ForEach(x => x.Dispose());

            return res;
        }
        private async Task<int> GetCountAsync(IQueryable newSource)
        {
            var results = await GetStatisDataAsync<int>(x => EntityFrameworkQueryableExtensions.CountAsync((dynamic)x), newSource);
            return results.Sum();
        }
        private async Task<TResult> GetSumAsync<TResult>(IQueryable<TResult> newSource)
        {
            var results = await GetStatisDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.SumAsync((dynamic)x), newSource);
            return Enumerable.Sum((dynamic)results);
        }
        private async Task<TResult> GetSumAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            return await GetSumAsync(newSource);
        }
        private async Task<dynamic> GetDynamicAverageAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            //总数量
            var allCount = await GetCountAsync(newSource);

            //总合
            var sum = await GetSumAsync(newSource);
            if (sum is int || sum is int? || sum is long || sum is long?)
                return ((double?)(dynamic)sum) / allCount;
            else
                return (dynamic)sum / allCount;
        }

        #endregion

        #region 外部接口

        public IShardingQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            _source = _source.Where(predicate);

            return this;
        }
        public IShardingQueryable<T> Where(string predicate, params object[] values)
        {
            _source = _source.Where(predicate, values);

            return this;
        }
        public IShardingQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderBy(keySelector);

            return this;
        }
        public IShardingQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderByDescending(keySelector);

            return this;
        }
        public IShardingQueryable<T> OrderBy(string ordering, params object[] values)
        {
            _source = _source.OrderBy(ordering, values);

            return this;
        }
        public IShardingQueryable<T> Skip(int count)
        {
            _source = _source.Skip(count);

            return this;
        }
        public IShardingQueryable<T> Take(int count)
        {
            _source = _source.Take(count);

            return this;
        }
        public int Count()
        {
            return AsyncHelper.RunSync(() => CountAsync());
        }
        public async Task<int> CountAsync()
        {
            return await GetCountAsync(_source);
        }
        public List<T> ToList()
        {
            return AsyncHelper.RunSync(() => ToListAsync());
        }
        public async Task<List<T>> ToListAsync()
        {
            //去除分页,获取前Take+Skip数量
            int? take = _source.GetTakeCount();
            int? skip = _source.GetSkipCount();
            skip = skip == null ? 0 : skip;
            var (sortColumn, sortType) = _source.GetOrderBy();
            var noPaginSource = _source.RemoveTake().RemoveSkip();
            if (!take.IsNullOrEmpty())
                noPaginSource = noPaginSource.Take(take.Value + skip.Value);

            //从各个分表获取数据
            var tables = ShardingConfig.ConfigProvider.GetReadTables(_absTableName, _absDbName, _source);
            SynchronizedCollection<IDbAccessor> dbs = new SynchronizedCollection<IDbAccessor>();
            List<Task<List<T>>> tasks = tables.Select(aTable =>
            {
                var targetTable = MapTable(aTable.tableName);
                IDbAccessor db;
                if (_repository.OpenedTransaction)
                    db = _repository.GetMapDbAccessor(aTable.conString, aTable.dbType);
                else
                    db = DbFactory.GetDbAccessor(aTable.conString, aTable.dbType);
                dbs.Add(db);

                var targetIQ = db.GetIQueryable(targetTable);
                var newQ = noPaginSource.ChangeSource(targetIQ);
                return newQ
                    .Cast<object>()
                    .Select(x => x.ChangeType<T>())
                    .ToListAsync();
            }).ToList();
            List<T> all = new List<T>();
            (await Task.WhenAll(tasks.ToArray())).ToList().ForEach(x => all.AddRange(x));

            if (!_repository.OpenedTransaction)
                dbs.ForEach(x => x.Dispose());
            //合并数据
            var resList = all;
            if (!sortColumn.IsNullOrEmpty() && !sortType.IsNullOrEmpty())
                resList = resList.AsQueryable().OrderBy($"{sortColumn} {sortType}").ToList();
            if (!skip.IsNullOrEmpty())
                resList = resList.Skip(skip.Value).ToList();
            if (!take.IsNullOrEmpty())
                resList = resList.Take(take.Value).ToList();

            return resList;
        }
        public T FirstOrDefault()
        {
            return AsyncHelper.RunSync(() => FirstOrDefaultAsync());
        }
        public async Task<T> FirstOrDefaultAsync()
        {
            var list = await GetStatisDataAsync(async x =>
            {
                var data = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((dynamic)x);
                return (data as object)?.ChangeType<T>();
            });
            list.RemoveAll(x => x == null);
            var q = list.AsQueryable();
            var (sortColumn, sortType) = _source.GetOrderBy();
            if (!sortColumn.IsNullOrEmpty())
                q = q.OrderBy($"{sortColumn} {sortType}");

            return q.FirstOrDefault();
        }
        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return AsyncHelper.RunSync(() => MaxAsync(selector));
        }
        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            var results = await GetStatisDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.MaxAsync((dynamic)x), newSource);

            return results.Max();
        }
        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            return AsyncHelper.RunSync(() => MinAsync(selector));
        }
        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            var results = await GetStatisDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.MinAsync((dynamic)x), newSource);

            return results.Min();
        }
        public double Average(Expression<Func<T, int>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, int>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<float> AverageAsync(Expression<Func<T, float>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, long>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, double>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            return await GetSumAsync(selector);
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<double> SumAsync(Expression<Func<T, double>> selector)
        {
            return await GetSumAsync(selector);
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<float> SumAsync(Expression<Func<T, float>> selector)
        {
            return await GetSumAsync(selector);
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public int Sum(Expression<Func<T, int>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<int> SumAsync(Expression<Func<T, int>> selector)
        {
            return await GetSumAsync(selector);
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<long> SumAsync(Expression<Func<T, long>> selector)
        {
            return await GetSumAsync(selector);
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return AsyncHelper.RunSync(() => AnyAsync(predicate));
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            var newSource = _source.Where(predicate);
            return (await GetStatisDataAsync<bool>(x => EntityFrameworkQueryableExtensions.AnyAsync((dynamic)x), newSource))
                .Any(x => x == true);
        }

        #endregion
    }
}
