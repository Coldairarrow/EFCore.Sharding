using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class ReadWriteDbAccessor : IDbAccessor
    {
        #region 私有成员

        private readonly IServiceProvider _serviceProvider;
        private readonly (string connectionString, DatabaseType dbType)[] _dbs;
        private readonly string _entityNamespace;
        public ReadWriteDbAccessor(IServiceProvider serviceProvider, (string connectionString, DatabaseType dbType)[] dbs, string entityNamespace)
        {
            _serviceProvider = serviceProvider;
            _dbs = dbs;
            _entityNamespace = entityNamespace;
        }
        private IDbAccessor Db { get; }

        #endregion

        #region 事务

        public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Db.RunTransaction(action, isolationLevel);
        }
        public Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Db.RunTransactionAsync(action, isolationLevel);
        }
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            Db.BeginTransaction(isolationLevel);
        }
        public Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return Db.BeginTransactionAsync(isolationLevel);
        }
        public void RollbackTransaction()
        {
            Db.RollbackTransaction();
        }
        public void CommitTransaction()
        {
            Db.CommitTransaction();
        }

        #endregion

        #region 外部属性

        public string ConnectionString => throw new Exception("读写分离模式不支持");
        public DatabaseType DbType => throw new Exception("读写分离模式不支持");
        public IDbAccessor FullDbAccessor => throw new Exception("读写分离模式不支持");

        #endregion

        #region 增

        #endregion
        public void BulkInsert<T>(List<T> entities) where T : class
        {
            Db.BulkInsert(entities);
        }

        public int Delete(Type type, string key)
        {
            return Db.Delete(type, key);
        }

        public int Delete(Type type, List<string> keys)
        {
            return Db.Delete(type, keys);
        }

        public int Delete<T>(string key) where T : class
        {
            return Db.Delete<T>(key);
        }

        public int Delete<T>(List<string> keys) where T : class
        {
            return Db.Delete<T>(keys);
        }

        public int Delete<T>(T entity) where T : class
        {
            return Db.Delete(entity);
        }

        public int Delete<T>(List<T> entities) where T : class
        {
            return Db.Delete(entities);
        }

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return Db.Delete(condition);
        }

        public int DeleteAll(Type type)
        {
            return Db.DeleteAll(type);
        }

        public int DeleteAll<T>() where T : class
        {
            return Db.DeleteAll<T>();
        }

        public Task<int> DeleteAllAsync(Type type)
        {
            return Db.DeleteAllAsync(type);
        }

        public Task<int> DeleteAllAsync<T>() where T : class
        {
            return Db.DeleteAllAsync<T>();
        }

        public Task<int> DeleteAsync(Type type, string key)
        {
            return Db.DeleteAsync(type, key);
        }

        public Task<int> DeleteAsync(Type type, List<string> keys)
        {
            return Db.DeleteAsync(type, keys);
        }

        public Task<int> DeleteAsync<T>(string key) where T : class
        {
            return Db.DeleteAsync<T>(key);
        }

        public Task<int> DeleteAsync<T>(List<string> keys) where T : class
        {
            return Db.DeleteAsync<T>(keys);
        }

        public Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return Db.DeleteAsync(entity);
        }

        public Task<int> DeleteAsync<T>(List<T> entities) where T : class
        {
            return Db.DeleteAsync(entities);
        }

        public Task<int> DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return Db.DeleteAsync(condition);
        }

        public int Delete_Sql<T>(Expression<Func<T, bool>> where) where T : class
        {
            return Db.Delete_Sql(where);
        }

        public int Delete_Sql(Type entityType, string where, params object[] paramters)
        {
            return Db.Delete_Sql(entityType, where, paramters);
        }

        public int Delete_Sql(IQueryable source)
        {
            return Db.Delete_Sql(source);
        }

        public Task<int> Delete_SqlAsync<T>(Expression<Func<T, bool>> where) where T : class
        {
            return Db.Delete_SqlAsync(where);
        }

        public Task<int> Delete_SqlAsync(Type entityType, string where, params object[] paramters)
        {
            return Db.Delete_SqlAsync(entityType, where, paramters);
        }

        public Task<int> Delete_SqlAsync(IQueryable source)
        {
            return Db.Delete_SqlAsync(source);
        }

        public void Dispose()
        {

        }

        public void DisposeTransaction()
        {
            Db.DisposeTransaction();
        }

        public int ExecuteSql(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return Db.ExecuteSql(sql, parameters);
        }

        public Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return Db.ExecuteSqlAsync(sql, parameters);
        }

        public DataTable GetDataTableWithSql(string sql, params (string paramterName, object value)[] parameters)
        {
            return Db.GetDataTableWithSql(sql, parameters);
        }

        public Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            return Db.GetDataTableWithSqlAsync(sql, parameters);
        }

        public T GetEntity<T>(params object[] keyValue) where T : class
        {
            return Db.GetEntity<T>(keyValue);
        }

        public Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class
        {
            return Db.GetEntityAsync<T>(keyValue);
        }

        public IQueryable<T> GetIQueryable<T>() where T : class
        {
            return Db.GetIQueryable<T>();
        }

        public IQueryable GetIQueryable(Type type)
        {
            return Db.GetIQueryable(type);
        }

        public List<object> GetList(Type type)
        {
            return Db.GetList(type);
        }

        public List<T> GetList<T>() where T : class
        {
            return Db.GetList<T>();
        }

        public Task<List<object>> GetListAsync(Type type)
        {
            return Db.GetListAsync(type);
        }

        public Task<List<T>> GetListAsync<T>() where T : class
        {
            return Db.GetListAsync<T>();
        }

        public List<T> GetListBySql<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            return Db.GetListBySql<T>(sqlStr, parameters);
        }

        public Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            return Db.GetListBySqlAsync<T>(sqlStr, parameters);
        }

        public int Insert<T>(T entity) where T : class
        {
            return Db.Insert(entity);
        }

        public int Insert<T>(List<T> entities) where T : class
        {
            return Db.Insert(entities);
        }

        public Task<int> InsertAsync<T>(T entity) where T : class
        {
            return Db.InsertAsync(entity);
        }

        public Task<int> InsertAsync<T>(List<T> entities) where T : class
        {
            return Db.InsertAsync(entities);
        }

        public int Update<T>(T entity) where T : class
        {
            return Db.Update(entity);
        }

        public int Update<T>(List<T> entities) where T : class
        {
            return Db.Update(entities);
        }

        public int Update<T>(T entity, List<string> properties) where T : class
        {
            return Db.Update(entity, properties);
        }

        public int Update<T>(List<T> entities, List<string> properties) where T : class
        {
            return Db.Update(entities, properties);
        }

        public int Update<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class
        {
            return Db.Update(whereExpre, set);
        }

        public Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return Db.UpdateAsync(entity);
        }

        public Task<int> UpdateAsync<T>(List<T> entities) where T : class
        {
            return Db.UpdateAsync(entities);
        }

        public Task<int> UpdateAsync<T>(T entity, List<string> properties) where T : class
        {
            return Db.UpdateAsync(entity, properties);
        }

        public Task<int> UpdateAsync<T>(List<T> entities, List<string> properties) where T : class
        {
            return Db.UpdateAsync(entities, properties);
        }

        public Task<int> UpdateAsync<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class
        {
            return Db.UpdateAsync(whereExpre, set);
        }

        public int Update_Sql<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return Db.Update_Sql(where, values);
        }

        public int Update_Sql(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return Db.Update_Sql(entityType, where, paramters, values);
        }

        public int Update_Sql(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return Db.Update_Sql(source, values);
        }

        public Task<int> Update_SqlAsync<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return Db.Update_SqlAsync(where, values);
        }

        public Task<int> Update_SqlAsync(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return Db.Update_SqlAsync(entityType, where, paramters, values);
        }

        public Task<int> Update_SqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return Db.Update_SqlAsync(source, values);
        }
    }
}
