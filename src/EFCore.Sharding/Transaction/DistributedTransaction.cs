using EFCore.Sharding.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库分布式事务,跨库事务
    /// </summary>
    internal class DistributedTransaction : IDistributedTransaction
    {
        #region 内部成员

        private IsolationLevel _isolationLevel { get; set; }
        private SynchronizedCollection<IDbAccessor> _repositories { get; set; }
            = new SynchronizedCollection<IDbAccessor>();

        #endregion

        #region 外部接口

        public bool OpenTransaction { get; set; }

        public void AddDbAccessor(params IDbAccessor[] repositories)
        {
            repositories.ForEach(aRepositroy =>
            {
                if (!_repositories.Contains(aRepositroy))
                {
                    if (OpenTransaction)
                        aRepositroy.BeginTransaction(_isolationLevel);

                    _repositories.Add(aRepositroy);
                }
            });
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            OpenTransaction = true;
            _isolationLevel = isolationLevel;
            _repositories.ForEach(aDbAccessor => aDbAccessor.BeginTransaction(isolationLevel));
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            OpenTransaction = true;
            _isolationLevel = isolationLevel;
            foreach (var aDbAccessor in _repositories)
            {
                await aDbAccessor.BeginTransactionAsync(isolationLevel);
            }
        }

        public (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_repositories.Count == 0)
                throw new Exception("IDbAccessor数量不能为0");

            bool isOK = true;
            Exception resEx = null;
            try
            {
                BeginTransaction(isolationLevel);

                action();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                isOK = false;
                resEx = ex;
            }
            finally
            {
                DisposeTransaction();
            }

            return (isOK, resEx);
        }

        public void CommitTransaction()
        {
            _repositories.ForEach(x => x.CommitTransaction());
        }

        public void RollbackTransaction()
        {
            _repositories.ForEach(x => x.RollbackTransaction());
        }

        public void DisposeTransaction()
        {
            OpenTransaction = false;
            _repositories.ForEach(x => x.DisposeTransaction());
        }

        public async Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_repositories.Count == 0)
                throw new Exception("IDbAccessor数量不能为0");

            bool isOK = true;
            Exception resEx = null;
            try
            {
                await BeginTransactionAsync(isolationLevel);

                await action();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                isOK = false;
                resEx = ex;
            }
            finally
            {
                DisposeTransaction();
            }

            return (isOK, resEx);
        }

        #endregion

        #region Dispose

        private bool _disposed = false;
        public virtual void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            DisposeTransaction();
            _repositories = null;
        }

        #endregion
    }
}
