using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal abstract class DefaultBaseDbAccessor : IBaseDbAccessor
    {
        #region 已实现

        public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            bool success = true;
            Exception resEx = null;
            try
            {
                BeginTransaction(isolationLevel);

                action();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                success = false;
                resEx = ex;
                RollbackTransaction();
            }
            finally
            {
                DisposeTransaction();
            }

            return (success, resEx);
        }
        public async Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            bool success = true;
            Exception resEx = null;
            try
            {
                await BeginTransactionAsync(isolationLevel);

                await action();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                success = false;
                resEx = ex;
                RollbackTransaction();
            }
            finally
            {
                DisposeTransaction();
            }

            return (success, resEx);

        }
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            AsyncHelper.RunSync(() => BeginTransactionAsync(isolationLevel));
        }
        public int Delete<T>(T entity) where T : class
        {
            return Delete(new List<T> { entity });
        }
        public int Delete<T>(List<T> entities) where T : class
        {
            return AsyncHelper.RunSync(() => DeleteAsync(entities));
        }
        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return AsyncHelper.RunSync(() => DeleteAsync(condition));
        }
        public int DeleteAll<T>() where T : class
        {
            return AsyncHelper.RunSync(() => DeleteAllAsync<T>());
        }
        public Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return DeleteAsync(new List<T> { entity });
        }
        public int DeleteSql<T>(Expression<Func<T, bool>> where) where T : class
        {
            return AsyncHelper.RunSync(() => DeleteSqlAsync(where));
        }
        public int Insert<T>(T entity, bool tracking = false) where T : class
        {
            return Insert(new List<T> { entity }, tracking);
        }
        public int Insert<T>(List<T> entities, bool tracking = false) where T : class
        {
            return AsyncHelper.RunSync(() => InsertAsync(entities, tracking));
        }
        public Task<int> InsertAsync<T>(T entity, bool tracking = false) where T : class
        {
            return InsertAsync(new List<T> { entity }, tracking);
        }
        public int Update<T>(T entity, bool tracking = false) where T : class
        {
            return Update(new List<T> { entity }, tracking);
        }
        public int Update<T>(List<T> entities, bool tracking = false) where T : class
        {
            return AsyncHelper.RunSync(() => UpdateAsync(entities, tracking));
        }
        public int Update<T>(T entity, List<string> properties, bool tracking = false) where T : class
        {
            return Update(new List<T> { entity }, properties, tracking);
        }
        public int Update<T>(List<T> entities, List<string> properties, bool tracking = false) where T : class
        {
            return AsyncHelper.RunSync(() => UpdateAsync(entities, properties, tracking));
        }
        public int Update<T>(Expression<Func<T, bool>> whereExpre, Action<T> set, bool tracking = false) where T : class
        {
            return AsyncHelper.RunSync(() => UpdateAsync(whereExpre, set, tracking));
        }
        public Task<int> UpdateAsync<T>(T entity, bool tracking = false) where T : class
        {
            return UpdateAsync(new List<T> { entity }, tracking);
        }
        public Task<int> UpdateAsync<T>(T entity, List<string> properties, bool tracking = false) where T : class
        {
            return UpdateAsync(new List<T> { entity }, properties, tracking);
        }

        #endregion

        #region 待实现

        public abstract Task BeginTransactionAsync(IsolationLevel isolationLevel);
        public abstract void CommitTransaction();
        public abstract void DisposeTransaction();
        public abstract void RollbackTransaction();
        public abstract Task<int> DeleteAllAsync<T>() where T : class;
        public abstract Task<int> DeleteAsync<T>(List<T> entities) where T : class;
        public abstract Task<int> DeleteSqlAsync<T>(Expression<Func<T, bool>> where) where T : class;
        public abstract Task<int> DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : class;
        public abstract Task<int> InsertAsync<T>(List<T> entities, bool tracking = false) where T : class;
        public abstract Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false) where T : class;
        public abstract Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false) where T : class;
        public abstract Task<int> UpdateAsync<T>(Expression<Func<T, bool>> whereExpre, Action<T> set, bool tracking = false) where T : class;
        public abstract void Dispose();

        #endregion
    }
}
