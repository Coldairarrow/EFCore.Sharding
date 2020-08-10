using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class GenericDbAccessor : DefaultDbAccessor
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseDbContext">BaseDbContext</param>
        public GenericDbAccessor(GenericDbContext baseDbContext)
        {
            _db = baseDbContext;
            _provider = DbFactory.GetProvider(DbType);
        }

        #endregion

        #region 私有成员

        protected AbstractProvider _provider { get; }
        protected GenericDbContext _db { get; }
        protected IDbContextTransaction _transaction { get; set; }
        protected bool _openedTransaction { get; set; } = false;
        protected virtual string FormatFieldName(string name)
        {
            throw new NotImplementedException("请在子类实现!");
        }
        protected virtual string FormatParamterName(string name)
        {
            return $"@{name}";
        }
        protected virtual string GetSchema(string schema)
        {
            throw new Exception("请在子类实现");
        }
        private string GetFormatedSchemaAndTableName(Type entityType)
        {
            string fullName = string.Empty;
            string schema = AnnotationHelper.GetDbSchemaName(entityType);
            schema = GetSchema(schema);
            string table = AnnotationHelper.GetDbTableName(entityType);
            if (!_db.Options.Suffix.IsNullOrEmpty())
            {
                table += $"_{_db.Options.Suffix}";
            }

            if (schema.IsNullOrEmpty())
                fullName = FormatFieldName(table);
            else
                fullName = $"{FormatFieldName(schema)}.{FormatFieldName(table)}";

            return fullName;
        }
        private (string sql, List<(string paramterName, object paramterValue)> paramters) GetWhereSql(IQueryable query)
        {
            List<(string paramterName, object paramterValue)> paramters =
                new List<(string paramterName, object paramterValue)>();
            var querySql = query.ToSql();
            string theQSql = querySql.sql.Replace("\r\n", "\n").Replace("\n", " ");
            //无筛选
            if (!theQSql.Contains("WHERE"))
                return (" 1=1 ", paramters);

            string pattern1 = "^SELECT.*?FROM.*? AS (.*?) WHERE .*?$";
            string pattern2 = "^SELECT.*?FROM .*? (.*?) WHERE .*?$";
            string asTmp = string.Empty;
            if (Regex.IsMatch(theQSql, pattern1))
            {
                var match = Regex.Match(theQSql, pattern1);
                asTmp = match.Groups[1]?.ToString();
            }
            else if (Regex.IsMatch(theQSql, pattern2))
            {
                var match = Regex.Match(theQSql, pattern2);
                asTmp = match.Groups[1]?.ToString();
            }
            if (asTmp.IsNullOrEmpty())
                throw new Exception("SQL解析失败!");

            string whereSql = querySql.sql.Split(new string[] { "WHERE" }, StringSplitOptions.None)[1].Replace($"{asTmp}.", "");

            querySql.parameters.ForEach(aData =>
            {
                if (whereSql.Contains(aData.Key))
                    paramters.Add((aData.Key, aData.Value));
            });

            return (whereSql, paramters);
        }
        private (string sql, List<(string paramterName, object paramterValue)> paramters) GetDeleteSql(IQueryable iq)
        {
            string tableName = GetFormatedSchemaAndTableName(iq.ElementType);
            var whereSql = GetWhereSql(iq);
            string sql = $"DELETE FROM {tableName} WHERE {whereSql.sql}";

            return (sql, whereSql.paramters);
        }
        private (string sql, List<(string paramterName, object paramterValue)> paramters) GetUpdateWhereSql(IQueryable iq, params (string field, UpdateType updateType, object value)[] values)
        {
            string tableName = GetFormatedSchemaAndTableName(iq.ElementType);
            var whereSql = GetWhereSql(iq);

            List<string> propertySetStr = new List<string>();

            values.ToList().ForEach(aProperty =>
            {
                var paramterName = FormatParamterName($"_p_{aProperty.field}");
                string formatedField = FormatFieldName(aProperty.field);
                whereSql.paramters.Add((paramterName, aProperty.value));

                string setValueBody = string.Empty;
                switch (aProperty.updateType)
                {
                    case UpdateType.Equal: setValueBody = paramterName; break;
                    case UpdateType.Add: setValueBody = $" {formatedField} + {paramterName} "; break;
                    case UpdateType.Minus: setValueBody = $" {formatedField} - {paramterName} "; break;
                    case UpdateType.Multiply: setValueBody = $" {formatedField} * {paramterName} "; break;
                    case UpdateType.Divide: setValueBody = $" {formatedField} / {paramterName} "; break;
                    default: throw new Exception("updateType无效");
                }

                propertySetStr.Add($" {formatedField} = {setValueBody} ");
            });
            string sql = $"UPDATE {tableName} SET {string.Join(",", propertySetStr)} WHERE {whereSql.sql}";

            return (sql, whereSql.paramters);
        }
        private List<DbParameter> CreateDbParamters(List<(string paramterName, object paramterValue)> paramters)
        {
            List<DbParameter> dbParamters = new List<DbParameter>();
            paramters.ForEach(aParamter =>
            {
                var newParamter = _provider.GetDbParameter();
                newParamter.ParameterName = aParamter.paramterName;
                newParamter.Value = aParamter.paramterValue;
                dbParamters.Add(newParamter);
            });

            return dbParamters;
        }

        #endregion

        #region 事物相关

        public override async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            _openedTransaction = true;
            _transaction = await _db.Database.BeginTransactionAsync(isolationLevel);
        }
        public override void CommitTransaction()
        {
            _transaction?.Commit();
        }
        public override void DisposeTransaction()
        {
            _db.Detach();
            _transaction?.Dispose();
            _openedTransaction = false;
        }
        public override void RollbackTransaction()
        {
            _transaction?.Rollback();
        }

        #endregion

        #region 数据库相关

        public override string ConnectionString => _db.Options.ConnectionString;
        public override DatabaseType DbType => _db.Options.DbType;
        public override IDbAccessor FullDbAccessor => this;
        public override async Task<int> SaveChangesAsync(bool tracking = true)
        {
            int count = await _db.SaveChangesAsync();
            if (!tracking)
            {
                _db.Detach();
            }

            return count;
        }

        #endregion

        #region 增加数据

        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            throw new Exception("待支持");
        }
        public override async Task<int> InsertAsync<T>(List<T> entities, bool tracking = false)
        {
            await _db.AddRangeAsync(entities);

            return await SaveChangesAsync(tracking);
        }

        #endregion

        #region 删除数据

        public override async Task<int> DeleteSqlAsync(IQueryable source)
        {
            var sql = GetDeleteSql(source);

            return await ExecuteSqlAsync(sql.sql, sql.paramters.ToArray());
        }
        public override async Task<int> DeleteAsync<T>(List<T> entities)
        {
            _db.RemoveRange(entities);

            return await SaveChangesAsync(false);
        }

        #endregion

        #region 更新数据

        public override async Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false)
        {
            entities.ForEach(aEntity =>
            {
                _db.Entry(aEntity).State = EntityState.Modified;
            });

            return await SaveChangesAsync(tracking);
        }
        public override async Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false)
        {
            entities.ForEach(aEntity =>
            {
                properties.ForEach(aProperty =>
                {
                    _db.Entry(aEntity).Property(aProperty).IsModified = true;
                });
            });

            return await SaveChangesAsync(tracking);
        }
        public override async Task<int> UpdateSqlAsync(IQueryable source, params (string field, UpdateType updateType, object value)[] values)
        {
            var sql = GetUpdateWhereSql(source, values);

            return await ExecuteSqlAsync(sql.sql, sql.paramters.ToArray());
        }

        #endregion

        #region 查询数据

        public override async Task<T> GetEntityAsync<T>(params object[] keyValue)
        {
            var obj = await _db.Set<T>().FindAsync(keyValue);
            if (!obj.IsNullOrEmpty())
                _db.Entry(obj).State = EntityState.Detached;

            return obj;
        }
        public override IQueryable<T> GetIQueryable<T>(bool tracking = false)
        {
            var q = _db.Set<T>() as IQueryable<T>;

            if (!tracking)
                q = q.AsNoTracking();

            return q;
        }
        public override async Task<DataTable> GetDataTableWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            using DbConnection conn = _provider.GetDbConnection();
            conn.ConnectionString = ConnectionString;
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using DbCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 5 * 60;
            if (_openedTransaction)
            {
                cmd.Transaction = _transaction.GetDbTransaction();
            }

            if (parameters != null && parameters.Count() > 0)
                cmd.Parameters.AddRange(CreateDbParamters(parameters.ToList()).ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            DataTable table = new DataTable();

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(table);
            dataSet.EnforceConstraints = false;
            table.Load(reader);

            return table;
        }

        #endregion

        #region 执行Sql语句

        public override async Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
#if EFCORE3
            return await _db.Database.ExecuteSqlRawAsync(sql, CreateDbParamters(parameters.ToList()).ToArray());
#endif

#if EFCORE2
            return await _db.Database.ExecuteSqlCommandAsync(sql, CreateDbParamters(parameters.ToList()).ToArray());
#endif
        }

        #endregion

        #region Dispose

        private bool _disposed = false;
        public override void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            DisposeTransaction();
            _db.Dispose();
        }

        #endregion
    }
}
