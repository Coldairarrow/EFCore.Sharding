using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal abstract class DefaultDbAccessor : DefaultBaseDbAccessor, IDbAccessor
    {
        #region 已实现

        public int Delete<T>(string key) where T : class
        {
            return Delete<T>(new List<string> { key });
        }
        public int Delete<T>(List<string> keys) where T : class
        {
            return AsyncHelper.RunSync(() => DeleteAsync<T>(keys));
        }
        public Task<int> DeleteAsync<T>(string key) where T : class
        {
            return DeleteAsync<T>(new List<string> { key });
        }
        public int DeleteSql(IQueryable source)
        {
            return AsyncHelper.RunSync(() => DeleteSqlAsync(source));
        }
        public int ExecuteSql(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return AsyncHelper.RunSync(() => ExecuteSqlAsync(sql, parameters));
        }
        public DataTable GetDataTableWithSql(string sql, params (string paramterName, object value)[] parameters)
        {
            return AsyncHelper.RunSync(() => GetDataTableWithSqlAsync(sql, parameters));
        }
        public T GetEntity<T>(params object[] keyValue) where T : class
        {
            return AsyncHelper.RunSync(() => GetEntityAsync<T>(keyValue));
        }
        public List<T> GetListBySql<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            return AsyncHelper.RunSync(() => GetListBySqlAsync<T>(sqlStr, parameters));
        }
        public int SaveChanges(bool tracking = true)
        {
            return AsyncHelper.RunSync(() => SaveChangesAsync(tracking));
        }
        public int UpdateSql<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return AsyncHelper.RunSync(() => UpdateSqlAsync(where, values));
        }
        public int UpdateSql(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return AsyncHelper.RunSync(() => UpdateSqlAsync(source, values));
        }
        public Task<int> UpdateSqlAsync<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return UpdateSqlAsync(GetIQueryable<T>(), values);
        }
        public async Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            var table = await GetDataTableWithSqlAsync(sqlStr, parameters);

            return table.ToList<T>();
        }

        #endregion

        #region 待实现

        public abstract string ConnectionString { get; }
        public abstract DatabaseType DbType { get; }
        public abstract IDbAccessor FullDbAccessor { get; }
        public abstract void BulkInsert<T>(List<T> entities, string tableName = null) where T : class;
        public abstract Task<int> DeleteAsync<T>(List<string> keys) where T : class;
        public abstract Task<int> DeleteSqlAsync(IQueryable source);
        public abstract Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters);
        public abstract Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters);
        public abstract Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class;
        public abstract IQueryable<T> GetIQueryable<T>(bool tracking = false) where T : class;
        public abstract Task<int> SaveChangesAsync(bool tracking = true);
        public abstract Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values);

        #endregion
    }
}
