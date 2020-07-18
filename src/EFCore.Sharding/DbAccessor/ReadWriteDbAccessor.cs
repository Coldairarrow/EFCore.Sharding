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

        private readonly (string connectionString, ReadWriteType readWriteType)[] _dbConfigs;
        private readonly DatabaseType _dbType;
        private readonly string _entityNamespace;
        private readonly bool _logicDelete;
        private readonly IDbFactory _dbFactory;
        private readonly IShardingConfig _shardingConfig;
        public ReadWriteDbAccessor(
            (string connectionString, ReadWriteType readWriteType)[] dbs,
            DatabaseType dbType,
            string entityNamespace,
            IDbFactory dbFactory,
            IShardingConfig shardingConfig
            )
        {
            _dbConfigs = dbs;
            _entityNamespace = entityNamespace;
            _dbType = dbType;
            _logicDelete = shardingConfig.LogicDelete;
            _dbFactory = dbFactory;
            _shardingConfig = shardingConfig;
        }

        private (IDbAccessor db, ReadWriteType readWriteType)[] _allDbs;
        private (IDbAccessor db, ReadWriteType readWriteType)[] AllDbs
        {
            get
            {
                if (_allDbs == null)
                {
                    _allDbs = _dbConfigs
                        .Select(x => (_dbFactory.GetDbAccessor(x.connectionString, _dbType, _entityNamespace), x.readWriteType))
                        .ToArray();
                }

                return _allDbs;
            }
        }
        private IDbAccessor GetRandomDb(ReadWriteType readWriteType)
        {
            var dbs = AllDbs.Where(x => x.readWriteType.HasFlag(readWriteType)).ToList();

            var theDb = RandomHelper.Next(dbs).db;

            if (_logicDelete)
                theDb = new LogicDeleteDbAccessor(theDb, _shardingConfig);

            return theDb;
        }
        private IDbAccessor _writeDb;
        private IDbAccessor _readDb;
        private IDbAccessor WriteDb
        {
            get
            {
                if (_writeDb == null)
                {
                    _writeDb = GetRandomDb(ReadWriteType.Write);
                }

                return _writeDb;
            }
        }
        private IDbAccessor ReadDb
        {
            get
            {
                if (_openedTransaction)
                {
                    return WriteDb;
                }
                else
                {
                    if (_readDb == null)
                    {
                        _readDb = GetRandomDb(ReadWriteType.Read);
                    }

                    return _readDb;
                }
            }
        }
        private bool _openedTransaction = false;

        #endregion

        #region 事务

        public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _openedTransaction = true;
            var res = WriteDb.RunTransaction(action, isolationLevel);
            _openedTransaction = false;

            return res;
        }
        public async Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _openedTransaction = true;
            var res = await WriteDb.RunTransactionAsync(action, isolationLevel);
            _openedTransaction = false;

            return res;
        }
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            WriteDb.BeginTransaction(isolationLevel);
            _openedTransaction = true;
        }
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            await WriteDb.BeginTransactionAsync(isolationLevel);
            _openedTransaction = true;
        }
        public void RollbackTransaction()
        {
            WriteDb.RollbackTransaction();
        }
        public void CommitTransaction()
        {
            WriteDb.CommitTransaction();
        }
        public void DisposeTransaction()
        {
            WriteDb.DisposeTransaction();
            _openedTransaction = false;
        }

        #endregion

        #region 外部属性

        public string ConnectionString => throw new Exception("读写分离模式不支持");
        public DatabaseType DbType => throw new Exception("读写分离模式不支持");
        public IDbAccessor FullDbAccessor => throw new Exception("读写分离模式不支持");

        #endregion

        #region 增

        public void BulkInsert<T>(List<T> entities) where T : class
        {
            WriteDb.BulkInsert(entities);
        }
        public int Insert<T>(T entity) where T : class
        {
            return WriteDb.Insert(entity);
        }
        public int Insert<T>(List<T> entities) where T : class
        {
            return WriteDb.Insert(entities);
        }
        public Task<int> InsertAsync<T>(T entity) where T : class
        {
            return WriteDb.InsertAsync(entity);
        }
        public Task<int> InsertAsync<T>(List<T> entities) where T : class
        {
            return WriteDb.InsertAsync(entities);
        }

        #endregion

        #region 删

        public int Delete(Type type, string key)
        {
            return WriteDb.Delete(type, key);
        }
        public int Delete(Type type, List<string> keys)
        {
            return WriteDb.Delete(type, keys);
        }
        public int Delete<T>(string key) where T : class
        {
            return WriteDb.Delete<T>(key);
        }
        public int Delete<T>(List<string> keys) where T : class
        {
            return WriteDb.Delete<T>(keys);
        }
        public int Delete<T>(T entity) where T : class
        {
            return WriteDb.Delete(entity);
        }
        public int Delete<T>(List<T> entities) where T : class
        {
            return WriteDb.Delete(entities);
        }
        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return WriteDb.Delete(condition);
        }
        public int DeleteAll(Type type)
        {
            return WriteDb.DeleteAll(type);
        }
        public int DeleteAll<T>() where T : class
        {
            return WriteDb.DeleteAll<T>();
        }
        public Task<int> DeleteAllAsync(Type type)
        {
            return WriteDb.DeleteAllAsync(type);
        }
        public Task<int> DeleteAllAsync<T>() where T : class
        {
            return WriteDb.DeleteAllAsync<T>();
        }
        public Task<int> DeleteAsync(Type type, string key)
        {
            return WriteDb.DeleteAsync(type, key);
        }
        public Task<int> DeleteAsync(Type type, List<string> keys)
        {
            return WriteDb.DeleteAsync(type, keys);
        }
        public Task<int> DeleteAsync<T>(string key) where T : class
        {
            return WriteDb.DeleteAsync<T>(key);
        }
        public Task<int> DeleteAsync<T>(List<string> keys) where T : class
        {
            return WriteDb.DeleteAsync<T>(keys);
        }
        public Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return WriteDb.DeleteAsync(entity);
        }
        public Task<int> DeleteAsync<T>(List<T> entities) where T : class
        {
            return WriteDb.DeleteAsync(entities);
        }
        public Task<int> DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return WriteDb.DeleteAsync(condition);
        }
        public int Delete_Sql<T>(Expression<Func<T, bool>> where) where T : class
        {
            return WriteDb.Delete_Sql(where);
        }
        public int Delete_Sql(Type entityType, string where, params object[] paramters)
        {
            return WriteDb.Delete_Sql(entityType, where, paramters);
        }
        public int Delete_Sql(IQueryable source)
        {
            return WriteDb.Delete_Sql(source);
        }
        public Task<int> Delete_SqlAsync<T>(Expression<Func<T, bool>> where) where T : class
        {
            return WriteDb.Delete_SqlAsync(where);
        }
        public Task<int> Delete_SqlAsync(Type entityType, string where, params object[] paramters)
        {
            return WriteDb.Delete_SqlAsync(entityType, where, paramters);
        }
        public Task<int> Delete_SqlAsync(IQueryable source)
        {
            return WriteDb.Delete_SqlAsync(source);
        }

        #endregion

        #region 改

        public int Update<T>(T entity) where T : class
        {
            return WriteDb.Update(entity);
        }
        public int Update<T>(List<T> entities) where T : class
        {
            return WriteDb.Update(entities);
        }
        public int Update<T>(T entity, List<string> properties) where T : class
        {
            return WriteDb.Update(entity, properties);
        }
        public int Update<T>(List<T> entities, List<string> properties) where T : class
        {
            return WriteDb.Update(entities, properties);
        }
        public int Update<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class
        {
            return WriteDb.Update(whereExpre, set);
        }
        public Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return WriteDb.UpdateAsync(entity);
        }
        public Task<int> UpdateAsync<T>(List<T> entities) where T : class
        {
            return WriteDb.UpdateAsync(entities);
        }
        public Task<int> UpdateAsync<T>(T entity, List<string> properties) where T : class
        {
            return WriteDb.UpdateAsync(entity, properties);
        }
        public Task<int> UpdateAsync<T>(List<T> entities, List<string> properties) where T : class
        {
            return WriteDb.UpdateAsync(entities, properties);
        }
        public Task<int> UpdateAsync<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class
        {
            return WriteDb.UpdateAsync(whereExpre, set);
        }
        public int Update_Sql<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return WriteDb.Update_Sql(where, values);
        }
        public int Update_Sql(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return WriteDb.Update_Sql(entityType, where, paramters, values);
        }
        public int Update_Sql(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return WriteDb.Update_Sql(source, values);
        }
        public Task<int> Update_SqlAsync<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class
        {
            return WriteDb.Update_SqlAsync(where, values);
        }
        public Task<int> Update_SqlAsync(Type entityType, string where, object[] paramters, params (string field, UpdateType updateType, object value)[] values)
        {
            return WriteDb.Update_SqlAsync(entityType, where, paramters, values);
        }
        public Task<int> Update_SqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return WriteDb.Update_SqlAsync(source, values);
        }

        #endregion

        #region 查

        public DataTable GetDataTableWithSql(string sql, params (string paramterName, object value)[] parameters)
        {
            return ReadDb.GetDataTableWithSql(sql, parameters);
        }
        public Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            return ReadDb.GetDataTableWithSqlAsync(sql, parameters);
        }
        public T GetEntity<T>(params object[] keyValue) where T : class
        {
            return ReadDb.GetEntity<T>(keyValue);
        }
        public Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class
        {
            return ReadDb.GetEntityAsync<T>(keyValue);
        }
        public IQueryable<T> GetIQueryable<T>() where T : class
        {
            return ReadDb.GetIQueryable<T>();
        }
        public IQueryable GetIQueryable(Type type)
        {
            return ReadDb.GetIQueryable(type);
        }
        public List<object> GetList(Type type)
        {
            return ReadDb.GetList(type);
        }
        public List<T> GetList<T>() where T : class
        {
            return ReadDb.GetList<T>();
        }
        public Task<List<object>> GetListAsync(Type type)
        {
            return ReadDb.GetListAsync(type);
        }
        public Task<List<T>> GetListAsync<T>() where T : class
        {
            return ReadDb.GetListAsync<T>();
        }
        public List<T> GetListBySql<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            return ReadDb.GetListBySql<T>(sqlStr, parameters);
        }
        public Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters) where T : class
        {
            return ReadDb.GetListBySqlAsync<T>(sqlStr, parameters);
        }

        #endregion

        #region SQL

        public int ExecuteSql(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return WriteDb.ExecuteSql(sql, parameters);
        }

        public Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return WriteDb.ExecuteSqlAsync(sql, parameters);
        }

        #endregion

        #region Dispose
        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _writeDb?.Dispose();
            _readDb?.Dispose();
        }

        #endregion
    }
}
