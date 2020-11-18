using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal abstract class DefaultDbAccessor : DefaultBaseDbAccessor, IDbAccessor
    {
        protected static List<PropertyInfo> GetKeyPropertys(Type type)
        {
            var properties = type
                .GetProperties()
                .Where(x => x.GetCustomAttributes(true).Select(o => o.GetType().FullName).Contains(typeof(KeyAttribute).FullName))
                .ToList();

            return properties;
        }
        private List<object> GetDeleteList(Type type, List<string> keys)
        {
            var theProperty = GetKeyPropertys(type).FirstOrDefault();
            if (theProperty == null)
                throw new Exception("该实体没有主键标识！请使用[Key]标识主键！");

            List<object> deleteList = new List<object>();
            keys.ForEach(aKey =>
            {
                object newData = Activator.CreateInstance(type);
                var value = aKey.ChangeType(theProperty.PropertyType);
                theProperty.SetValue(newData, value);
                deleteList.Add(newData);
            });

            return deleteList;
        }

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
        public DataSet GetDataSetWithSql(string sql, params (string paramterName, object value)[] parameters)
        {
            return AsyncHelper.RunSync(() => GetDataSetWithSqlAsync(sql, parameters));
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
            return UpdateSqlAsync(GetIQueryable<T>().Where(where), values);
        }
        public async Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            var table = await GetDataTableWithSqlAsync(sqlStr, parameters);

            return table.ToList<T>();
        }
        public async override Task<int> DeleteSqlAsync<T>(Expression<Func<T, bool>> where)
        {
            var iq = GetIQueryable<T>(false).Where(where);

            return await DeleteSqlAsync(iq);
        }
        public override async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> condition)
        {
            var list = await GetIQueryable<T>().Where(condition).ToListAsync();

            return await DeleteAsync(list);
        }
        public override async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> whereExpre, Action<T> set, bool tracking = false)
        {
            var list = await GetIQueryable<T>(false).Where(whereExpre).ToListAsync();

            list.ForEach(aData =>
            {
                set(aData);
            });

            return await UpdateAsync(list, tracking);
        }
        public override async Task<int> DeleteAllAsync<T>()
        {
            return await DeleteSqlAsync(GetIQueryable<T>());
        }
        public virtual async Task<int> DeleteAsync<T>(List<string> keys) where T : class
        {
            return await DeleteAsync(GetDeleteList(typeof(T), keys));
        }

        #endregion

        #region 待实现

        public abstract string ConnectionString { get; }
        public abstract DatabaseType DbType { get; }
        public abstract IDbAccessor FullDbAccessor { get; }
        public abstract void BulkInsert<T>(List<T> entities, string tableName = null) where T : class;
        public abstract Task<int> DeleteSqlAsync(IQueryable source);
        public abstract Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters);
        public abstract Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters);
        public abstract Task<DataSet> GetDataSetWithSqlAsync(string sql, params (string paramterName, object value)[] parameters);
        public abstract Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class;
        public abstract IQueryable<T> GetIQueryable<T>(bool tracking = false) where T : class;
        public abstract Task<int> SaveChangesAsync(bool tracking = true);
        public abstract Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values);
        public abstract EntityEntry Entry(object entity);

        #endregion
    }
}
