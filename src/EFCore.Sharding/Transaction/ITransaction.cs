using System;
using System.Data;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    ///事物操作接口
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// 执行事务,具体执行操作包括在action中
        /// 注:支持自定义事务级别,默认为ReadCommitted
        /// </summary>
        /// <param name="action">执行操作</param>
        /// <param name="isolationLevel">事务级别,默认为ReadCommitted</param>
        /// <returns></returns>
        (bool Success, Exception ex) RunTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// 执行事务,具体执行操作包括在action中
        /// 注:支持自定义事务级别,默认为ReadCommitted
        /// </summary>
        /// <param name="action">执行操作</param>
        /// <param name="isolationLevel">事务级别,默认为ReadCommitted</param>
        /// <returns></returns>
        Task<(bool Success, Exception ex)> RunTransactionAsync(Func<Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        void BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        Task BeginTransactionAsync(IsolationLevel isolationLevel);

        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// 释放事务
        /// </summary>
        void DisposeTransaction();
    }
}
