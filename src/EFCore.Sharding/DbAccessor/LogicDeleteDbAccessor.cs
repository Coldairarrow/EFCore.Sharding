using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// 软删除访问接口
    /// 软删除:查询:获取Deleted=false,删除:更新Deleted=true
    /// </summary>
    internal class LogicDeleteDbAccessor : IDbAccessor
    {
        public LogicDeleteDbAccessor(IDbAccessor db)
        {
            FullDbAccessor = db;
        }

        bool NeedLogicDelete(Type entityType)
        {
            return ShardingConfig.LogicDelete && entityType.GetProperties().Any(x => x.Name == ShardingConfig.DeletedField);
        }
        public Action<string> HandleSqlLog { set => FullDbAccessor.HandleSqlLog = value; }
        public string ConnectionString => FullDbAccessor.ConnectionString;
        public DatabaseType DbType => FullDbAccessor.DbType;
        public IDbAccessor FullDbAccessor { get; }
        private T LogicDeleteFilter<T>(T data)
        {
            return LogicDeleteFilter(new List<T> { data }).FirstOrDefault();
        }
        private List<T> LogicDeleteFilter<T>(List<T> list)
        {
            if (NeedLogicDelete(typeof(T)))
                return list.Where(x => !(bool)x.GetPropertyValue(ShardingConfig.DeletedField)).ToList();
            else
                return list;
        }

        #region 重写

        public IQueryable<T> GetIQueryable<T>() where T : class, new()
        {
            return GetIQueryable(typeof(T)) as IQueryable<T>;
        }
        public IQueryable GetIQueryable(Type type)
        {
            var q = FullDbAccessor.GetIQueryable(type);
            if (NeedLogicDelete(type))
            {
                q = q.Where($"{ShardingConfig.DeletedField} = @0", false);
            }

            return q;
        }
        public T GetEntity<T>(params object[] keyValue) where T : class, new()
        {
            var obj = FullDbAccessor.GetEntity<T>(keyValue);
            if (obj == null)
                return null;

            return LogicDeleteFilter(obj);
        }
        public async Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class, new()
        {
            var obj = await FullDbAccessor.GetEntityAsync<T>(keyValue);

            return LogicDeleteFilter(obj);
        }
        public List<object> GetList(Type type)
        {
            return GetIQueryable(type).CastToList<object>();
        }
        public async Task<List<object>> GetListAsync(Type type)
        {
            return await GetIQueryable(type).Cast<object>().ToListAsync();
        }
        public List<T> GetList<T>() where T : class, new()
        {
            return GetIQueryable<T>().ToList();
        }
        public Task<List<T>> GetListAsync<T>() where T : class, new()
        {
            return GetIQueryable<T>().ToListAsync();
        }
        public int Delete(Type type, string key)
        {
            return Delete(type, new List<string> { key });
        }
        public async Task<int> DeleteAsync(Type type, string key)
        {
            return await DeleteAsync(type, new List<string> { key });
        }
        public int Delete(Type type, List<string> keys)
        {
            var iq = GetIQueryable(type).Where($"@0.Contains({ShardingConfig.KeyField})", new object[] { keys });

            return Delete_Sql(iq);
        }
        public async Task<int> DeleteAsync(Type type, List<string> keys)
        {
            var iq = GetIQueryable(type).Where($"@0.Contains({ShardingConfig.KeyField})", new object[] { keys });

            return await Delete_SqlAsync(iq);
        }
        public int Delete<T>(string key) where T : class, new()
        {
            return Delete<T>(new List<string> { key });
        }
        public async Task<int> DeleteAsync<T>(string key) where T : class, new()
        {
            return await DeleteAsync<T>(new List<string> { key });
        }
        public int Delete<T>(List<string> keys) where T : class, new()
        {
            return Delete(typeof(T), keys);
        }
        public async Task<int> DeleteAsync<T>(List<string> keys) where T : class, new()
        {
            return await DeleteAsync(typeof(T), keys);
        }
        public int Delete<T>(T entity) where T : class, new()
        {
            return Delete(new List<T> { entity });
        }
        public async Task<int> DeleteAsync<T>(T entity) where T : class, new()
        {
            return await DeleteAsync(new List<T> { entity });
        }
        public int Delete<T>(List<T> entities) where T : class, new()
        {
            if (entities?.Count > 0)
            {
                var keys = entities.Select(x => x.GetPropertyValue(ShardingConfig.KeyField) as string).ToList();
                return Delete(typeof(T), keys);
            }
            else
                return 0;
        }
        public async Task<int> DeleteAsync<T>(List<T> entities) where T : class, new()
        {
            if (entities?.Count > 0)
            {
                var keys = entities.Select(x => x.GetPropertyValue(ShardingConfig.KeyField) as string).ToList();
                return await DeleteAsync(typeof(T), keys);
            }
            else
                return 0;
        }
        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return Delete_Sql(condition);
        }
        public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return await Delete_SqlAsync(condition);
        }
        public int DeleteAll(Type type)
        {
            return Delete_Sql(type, "true");
            //if (NeedLogicDelete(type))
            //    return UpdateWhere_Sql(type, "true", null, (Deleted, UpdateType.Equal, true));
            //else
            //    return _db.DeleteAll(type);
        }
        public async Task<int> DeleteAllAsync(Type type)
        {
            return await Delete_SqlAsync(type, "true");
        }
        public int DeleteAll<T>() where T : class, new()
        {
            return DeleteAll(typeof(T));
        }
        public async Task<int> DeleteAllAsync<T>() where T : class, new()
        {
            return await DeleteAllAsync(typeof(T));
        }
        public int Delete_Sql<T>(Expression<Func<T, bool>> where) where T : class, new()
        {
            var iq = GetIQueryable<T>().Where(where);
            return Delete_Sql(iq);
        }
        public async Task<int> Delete_SqlAsync<T>(Expression<Func<T, bool>> where) where T : class, new()
        {
            var iq = GetIQueryable<T>().Where(where);
            return await Delete_SqlAsync(iq);
        }
        public int Delete_Sql(Type entityType, string where, params object[] paramters)
        {
            var iq = GetIQueryable(entityType).Where(where, paramters);

            return Delete_Sql(iq);
        }
        public async Task<int> Delete_SqlAsync(Type entityType, string where, params object[] paramters)
        {
            var iq = GetIQueryable(entityType).Where(where, paramters);

            return await Delete_SqlAsync(iq);
        }
        public int Delete_Sql(IQueryable source)
        {
            if (NeedLogicDelete(source.ElementType))
                return UpdateWhere_Sql(source, (ShardingConfig.DeletedField, UpdateType.Equal, true));
            else
                return FullDbAccessor.Delete_Sql(source);
        }
        public async Task<int> Delete_SqlAsync(IQueryable source)
        {
            if (NeedLogicDelete(source.ElementType))
                return await UpdateWhere_SqlAsync(source, (ShardingConfig.DeletedField, UpdateType.Equal, true));
            else
                return await FullDbAccessor.Delete_SqlAsync(source);
        }

        #endregion

        #region 忽略

        public void BulkInsert<T>(List<T> entities) where T : class, new()
        {
            FullDbAccessor.BulkInsert(entities);
        }
        public int UpdateWhere_Sql<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class, new()
        {
            return FullDbAccessor.UpdateWhere_Sql(where, values);
        }
        public Task<int> UpdateWhere_SqlAsync<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class, new()
        {
            return FullDbAccessor.UpdateWhere_SqlAsync(where, values);
        }
        public int UpdateWhere_Sql(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateWhere_Sql(entityType, where, paramters, values);
        }
        public Task<int> UpdateWhere_SqlAsync(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateWhere_SqlAsync(entityType, where, paramters, values);
        }
        public DataTable GetDataTableWithSql(string sql, params (string paramterName, object value)[] parameters)
        {
            return FullDbAccessor.GetDataTableWithSql(sql, parameters);
        }
        public Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            return FullDbAccessor.GetDataTableWithSqlAsync(sql, parameters);
        }
        public List<T> GetListBySql<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class, new()
        {
            return FullDbAccessor.GetListBySql<T>(sqlStr, parameters);
        }
        public Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class, new()
        {
            return FullDbAccessor.GetListBySqlAsync<T>(sqlStr, parameters);
        }
        public int ExecuteSql(string sql, params (string paramterName, object paramterValue)[] paramters)
        {
            return FullDbAccessor.ExecuteSql(sql, paramters);
        }
        public Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] paramters)
        {
            return FullDbAccessor.ExecuteSqlAsync(sql, paramters);
        }
        public int Insert<T>(T entity) where T : class, new()
        {
            return FullDbAccessor.Insert(entity);
        }
        public Task<int> InsertAsync<T>(T entity) where T : class, new()
        {
            return FullDbAccessor.InsertAsync(entity);
        }
        public int Insert<T>(List<T> entities) where T : class, new()
        {
            return FullDbAccessor.Insert(entities);
        }
        public Task<int> InsertAsync<T>(List<T> entities) where T : class, new()
        {
            return FullDbAccessor.InsertAsync(entities);
        }
        public int Update<T>(T entity) where T : class, new()
        {
            return FullDbAccessor.Update(entity);
        }
        public Task<int> UpdateAsync<T>(T entity) where T : class, new()
        {
            return FullDbAccessor.UpdateAsync(entity);
        }
        public int Update<T>(List<T> entities) where T : class, new()
        {
            return FullDbAccessor.Update(entities);
        }
        public Task<int> UpdateAsync<T>(List<T> entities) where T : class, new()
        {
            return FullDbAccessor.UpdateAsync(entities);
        }
        public int UpdateAny<T>(T entity, List<string> properties) where T : class, new()
        {
            return FullDbAccessor.UpdateAny(entity, properties);
        }
        public Task<int> UpdateAnyAsync<T>(T entity, List<string> properties) where T : class, new()
        {
            return FullDbAccessor.UpdateAnyAsync(entity, properties);
        }
        public int UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new()
        {
            return FullDbAccessor.UpdateAny(entities, properties);
        }
        public Task<int> UpdateAnyAsync<T>(List<T> entities, List<string> properties) where T : class, new()
        {
            return FullDbAccessor.UpdateAnyAsync(entities, properties);
        }
        public int UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new()
        {
            return FullDbAccessor.UpdateWhere(whereExpre, set);
        }
        public Task<int> UpdateWhereAsync<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new()
        {
            return FullDbAccessor.UpdateWhereAsync(whereExpre, set);
        }
        public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return FullDbAccessor.RunTransaction(action, isolationLevel);
        }
        public int UpdateWhere_Sql(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateWhere_Sql(source, values);
        }
        public Task<int> UpdateWhere_SqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateWhere_SqlAsync(source, values);
        }
        public void Dispose()
        {
            FullDbAccessor.Dispose();
        }
        public Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return FullDbAccessor.RunTransactionAsync(action, isolationLevel);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            FullDbAccessor.BeginTransaction(isolationLevel);
        }

        public Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return FullDbAccessor.BeginTransactionAsync(isolationLevel);
        }

        public void CommitTransaction()
        {
            FullDbAccessor.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            FullDbAccessor.RollbackTransaction();
        }

        public void DisposeTransaction()
        {
            FullDbAccessor.DisposeTransaction();
        }

        #endregion
    }
}
