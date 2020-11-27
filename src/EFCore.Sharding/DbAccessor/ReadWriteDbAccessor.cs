using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class ReadWriteDbAccessor : DefaultDbAccessor, IDbAccessor
    {
        #region 私有成员

        private readonly (string connectionString, ReadWriteType readWriteType)[] _dbConfigs;
        private readonly DatabaseType _dbType;
        private readonly string _entityNamespace;
        private readonly bool _logicDelete;
        private readonly IDbFactory _dbFactory;
        private readonly EFCoreShardingOptions _shardingOptions;
        public ReadWriteDbAccessor(
            (string connectionString, ReadWriteType readWriteType)[] dbs,
            DatabaseType dbType,
            string entityNamespace,
            IDbFactory dbFactory,
            EFCoreShardingOptions shardingOptions
            )
        {
            _dbConfigs = dbs;
            _entityNamespace = entityNamespace;
            _dbType = dbType;
            _logicDelete = shardingOptions.LogicDelete;
            _dbFactory = dbFactory;
            _shardingOptions = shardingOptions;
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
                theDb = new LogicDeleteDbAccessor(theDb, _shardingOptions);

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
        private bool _disposed = false;

        #endregion
        public override EntityEntry Entry(object entity)
        {
            return WriteDb.Entry(entity);
        }
        public override string ConnectionString => throw new Exception("读写分离模式不支持");
        public override DatabaseType DbType => throw new Exception("读写分离模式不支持");
        public override IDbAccessor FullDbAccessor => throw new Exception("读写分离模式不支持");
        public override void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _writeDb?.Dispose();
            _readDb?.Dispose();
        }
        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            WriteDb.BulkInsert(entities, tableName);
        }
        public override Task<int> DeleteSqlAsync(IQueryable source)
        {
            return WriteDb.DeleteSqlAsync(source);
        }
        public override Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return WriteDb.ExecuteSqlAsync(sql, parameters);
        }
        public override Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            return ReadDb.GetDataTableWithSqlAsync(sql, parameters);
        }
        public override Task<DataSet> GetDataSetWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            return ReadDb.GetDataSetWithSqlAsync(sql, parameters);
        }
        public override Task<T> GetEntityAsync<T>(params object[] keyValue)
        {
            return ReadDb.GetEntityAsync<T>(keyValue);
        }
        public override IQueryable<T> GetIQueryable<T>(bool tracking = false)
        {
            var db = tracking ? WriteDb : ReadDb;

            return db.GetIQueryable<T>(tracking);
        }
        public override Task<int> SaveChangesAsync(bool tracking = true)
        {
            return WriteDb.SaveChangesAsync(tracking);
        }
        public override Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return WriteDb.UpdateSqlAsync(source, values);
        }
        public override Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return WriteDb.BeginTransactionAsync(isolationLevel);
        }
        public override void CommitTransaction()
        {
            WriteDb.CommitTransaction();
        }
        public override void DisposeTransaction()
        {
            WriteDb.DisposeTransaction();
        }
        public override void RollbackTransaction()
        {
            WriteDb.RollbackTransaction();
        }
        public override Task<int> DeleteAsync<T>(List<T> entities)
        {
            return WriteDb.DeleteAsync(entities);
        }
        public override Task<int> InsertAsync<T>(List<T> entities, bool tracking = false)
        {
            return WriteDb.InsertAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false)
        {
            return WriteDb.UpdateAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false)
        {
            return WriteDb.UpdateAsync(entities, properties, tracking);
        }
        public override Task<List<T>> GetListBySqlAsync<T>(string sqlStr, params (string paramterName, object value)[] parameters)
        {
            return ReadDb.GetListBySqlAsync<T>(sqlStr, parameters);
        }
    }
}
