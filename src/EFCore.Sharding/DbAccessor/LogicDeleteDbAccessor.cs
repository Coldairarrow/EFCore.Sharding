using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// 软删除访问接口
    /// 软删除:查询:获取Deleted=false,删除:更新Deleted=true
    /// </summary>
    internal class LogicDeleteDbAccessor : DefaultDbAccessor, IDbAccessor
    {
        private bool _logicDelete;
        private string _deletedField;
        private string _keyField;
        public LogicDeleteDbAccessor(IDbAccessor db, EFCoreShardingOptions shardingOptions)
        {
            FullDbAccessor = db;
            _logicDelete = shardingOptions.LogicDelete;
            _deletedField = shardingOptions.DeletedField;
            _keyField = shardingOptions.KeyField;
        }
        public override IDbAccessor FullDbAccessor { get; }
        bool NeedLogicDelete(Type entityType)
        {
            return _logicDelete && entityType.GetProperties().Any(x => x.Name == _deletedField);
        }

        #region 重写

        public override async Task<int> DeleteAsync<T>(List<string> keys)
        {
            var entities = await GetIQueryable<T>().Where($"@0.Contains({_keyField})", keys).ToListAsync();

            return await DeleteAsync(entities);
        }
        public override async Task<int> DeleteAsync<T>(List<T> entities)
        {
            if (entities?.Count > 0)
            {
                if (NeedLogicDelete(typeof(T)))
                {
                    entities.ForEach(aData =>
                    {
                        aData.SetPropertyValue(_deletedField, true);
                    });

                    return await UpdateAsync(entities);
                }
                else
                {
                    return await FullDbAccessor.DeleteAsync(entities);
                }
            }
            else
                return 0;
        }
        public override async Task<int> DeleteSqlAsync(IQueryable source)
        {
            if (NeedLogicDelete(source.ElementType))
                return await UpdateSqlAsync(source, (_deletedField, UpdateType.Equal, true));
            else
                return await FullDbAccessor.DeleteSqlAsync(source);
        }
        public override IQueryable<T> GetIQueryable<T>(bool tracking = false)
        {
            var q = FullDbAccessor.GetIQueryable<T>(tracking);
            if (NeedLogicDelete(typeof(T)))
            {
                q = q.Where($"{_deletedField} = @0", false);
            }

            return q;
        }
        public override EntityEntry Entry(object entity)
        {
            return FullDbAccessor.Entry(entity);
        }

        #endregion

        #region 忽略
        public override string ConnectionString => FullDbAccessor.ConnectionString;
        public override DatabaseType DbType => FullDbAccessor.DbType;
        public override Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return FullDbAccessor.BeginTransactionAsync(isolationLevel);
        }
        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            FullDbAccessor.BulkInsert(entities, tableName);
        }
        public override void CommitTransaction()
        {
            FullDbAccessor.CommitTransaction();
        }
        public override void Dispose()
        {
            FullDbAccessor.Dispose();
        }
        public override void DisposeTransaction()
        {
            FullDbAccessor.DisposeTransaction();
        }
        public override Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return FullDbAccessor.ExecuteSqlAsync(sql, parameters);
        }
        public override Task<T> GetEntityAsync<T>(params object[] keyValue)
        {
            return FullDbAccessor.GetEntityAsync<T>(keyValue);
        }
        public override Task<int> InsertAsync<T>(List<T> entities, bool tracking = false)
        {
            return FullDbAccessor.InsertAsync(entities, tracking);
        }
        public override void RollbackTransaction()
        {
            FullDbAccessor.RollbackTransaction();
        }
        public override Task<int> SaveChangesAsync(bool tracking = true)
        {
            return FullDbAccessor.SaveChangesAsync(tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false)
        {
            return FullDbAccessor.UpdateAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false)
        {
            return FullDbAccessor.UpdateAsync(entities, properties, tracking);
        }
        public override Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateSqlAsync(source, values);
        }

        #endregion
    }
}
