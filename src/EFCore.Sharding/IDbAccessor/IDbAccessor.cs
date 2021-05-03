using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    /// <summary>
    /// 操作接口
    /// </summary>
    public interface IDbAccessor : IBaseDbAccessor, ITransaction
    {
        #region 数据库相关

        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        DatabaseType DbType { get; }

        /// <summary>
        /// 获取完整DbAccessor,通过此接口可以操作逻辑删除的数据
        /// </summary>
        IDbAccessor FullDbAccessor { get; }

        /// <summary>
        /// 保存修改到数据库(需要GetIQueryable开启实体追踪)
        /// </summary>
        /// <param name="tracking">是否开启实体追踪</param>
        /// <returns></returns>
        int SaveChanges(bool tracking = true);

        /// <summary>
        /// 保存修改到数据库(需要GetIQueryable开启实体追踪)
        /// </summary>
        /// <param name="tracking">是否开启实体追踪</param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(bool tracking = true);

        /// <summary>
        /// 跟踪
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        EntityEntry Entry(object entity);

        #endregion

        #region 增加数据

        /// <summary>
        /// 使用Bulk批量导入,速度快
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entities">实体集合</param>
        /// <param name="tableName">自定义表名</param>
        void BulkInsert<T>(List<T> entities, string tableName = null) where T : class;

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除单条记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="key">主键</param>
        int Delete<T>(string key) where T : class;

        /// <summary>
        /// 删除单条记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="key">主键</param>
        Task<int> DeleteAsync<T>(string key) where T : class;

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="keys">多条记录主键集合</param>
        int Delete<T>(List<string> keys) where T : class;

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="keys">多条记录主键集合</param>
        Task<int> DeleteAsync<T>(List<string> keys) where T : class;

        /// <summary>
        /// 删除指定数据源
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        int DeleteSql(IQueryable source);

        /// <summary>
        /// 删除指定数据源
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        Task<int> DeleteSqlAsync(IQueryable source);

        #endregion

        #region 更新数据

        /// <summary>
        /// 使用SQL语句按照条件更新
        /// 用法:UpdateWhere_Sql"Base_User"(x=>x.Id == "Admin",("Name",UpdateType.Equal,"小明"))
        /// 注：生成的SQL类似于UPDATE [TABLE] SET [Name] = 'xxx' WHERE [Id] = 'Admin'
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="where">筛选条件</param>
        /// <param name="values">字段值设置</param>
        /// <returns>影响条数</returns>
        int UpdateSql<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class;

        /// <summary>
        /// 使用SQL语句按照条件更新
        /// 用法:UpdateWhere_Sql"Base_User"(x=>x.Id == "Admin",("Name",UpdateType.Equal,"小明"))
        /// 注：生成的SQL类似于UPDATE [TABLE] SET [Name] = 'xxx' WHERE [Id] = 'Admin'
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="where">筛选条件</param>
        /// <param name="values">字段值设置</param>
        /// <returns>影响条数</returns>
        Task<int> UpdateSqlAsync<T>(Expression<Func<T, bool>> where, params (string field, UpdateType updateType, object value)[] values) where T : class;

        /// <summary>
        /// 使用SQL语句按照条件更新
        /// 用法:UpdateWhere_Sql"Base_User"(x=>x.Id == "Admin",("Name",UpdateType.Equal,"小明"))
        /// 注：生成的SQL类似于UPDATE [TABLE] SET [Name] = 'xxx' WHERE [Id] = 'Admin'
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="values">字段值设置</param>
        /// <returns>影响条数</returns>
        int UpdateSql(IQueryable source, params (string field, UpdateType updateType, object value)[] values);

        /// <summary>
        /// 使用SQL语句按照条件更新
        /// 用法:UpdateWhere_Sql"Base_User"(x=>x.Id == "Admin",("Name",UpdateType.Equal,"小明"))
        /// 注：生成的SQL类似于UPDATE [TABLE] SET [Name] = 'xxx' WHERE [Id] = 'Admin'
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="values">字段值设置</param>
        /// <returns>影响条数</returns>
        Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values);

        #endregion

        #region 查询数据

        /// <summary>
        /// 获取单条记录
        /// 注:无实体跟踪
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        T GetEntity<T>(params object[] keyValue) where T : class;

        /// <summary>
        /// 获取单条记录
        /// 注:无实体跟踪
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        Task<T> GetEntityAsync<T>(params object[] keyValue) where T : class;

        /// <summary>
        /// 获取IQueryable
        /// 注:默认取消实体追踪
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="tracking">是否开启实体追踪</param>
        /// <returns></returns>
        IQueryable<T> GetIQueryable<T>(bool tracking = false) where T : class;

        #endregion

        #region 执行Sql语句

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        int ExecuteSql(string sql, params (string paramterName, object paramterValue)[] parameters);

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters);

        #endregion
    }
}
