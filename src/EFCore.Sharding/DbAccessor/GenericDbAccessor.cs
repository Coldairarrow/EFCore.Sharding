using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            if (!_db.Paramter.Suffix.IsNullOrEmpty())
            {
                table += $"_{_db.Paramter.Suffix}";
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
            var theSql = querySql.sql.Replace("\r\n", "\n").Replace("\n", " ");

            //替换AS
            var asPattern = "FROM (.*?) AS (.*?) ";
            //倒排防止别名出错
            var asMatchs = Regex.Matches(theSql, asPattern).Cast<Match>().Reverse();
            foreach (Match aMatch in asMatchs)
            {
                var tableName = aMatch.Groups[1].ToString();
                var asName = aMatch.Groups[2].ToString();

                theSql = theSql.Replace(aMatch.Groups[0].ToString(), $"FROM {tableName} ");
                theSql = theSql.Replace(asName + ".", tableName + ".");
            }

            //无筛选
            if (!theSql.Contains("WHERE"))
                return (" 1=1 ", paramters);

            var firstIndex = theSql.IndexOf("WHERE") + 5;
            string whereSql = theSql.Substring(firstIndex, theSql.Length - firstIndex);

            querySql.parameters?.ForEach(aData =>
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
        private DynamicParameters CreateDynamicParameters((string paramterName, object paramterValue)[] paramters)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();

            paramters?.ForEach(aParamter =>
            {
                dynamicParameters.Add(aParamter.paramterName, aParamter.paramterValue);
            });

            return dynamicParameters;
        }
        private List<DbParameter> CreateDbParamters((string paramterName, object paramterValue)[] paramters)
        {
            List<DbParameter> dbParamters = new List<DbParameter>();
            paramters?.ForEach(aParamter =>
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
            if (!_disposed)
            {
                _db.Detach();
            }

            _transaction?.Dispose();
            _openedTransaction = false;
        }
        public override void RollbackTransaction()
        {
            _transaction?.Rollback();
        }

        #endregion

        #region 数据库相关

        public override string ConnectionString => _db.Paramter.ConnectionString;
        public override DatabaseType DbType => _db.Paramter.DbType;
        public override IDbAccessor FullDbAccessor => this;
        public override async Task<int> SaveChangesAsync(bool tracking = true)
        {
            int count = await _db.SaveChangesAsync();
            if (!tracking && !_openedTransaction)
            {
                _db.Detach();
            }

            return count;
        }
        public override EntityEntry Entry(object entity)
        {
            return _db.Entry(entity);
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
            var conn = _db.Database.GetDbConnection();
            using var reader = await conn.ExecuteReaderAsync(
                sql, CreateDynamicParameters(parameters), _transaction?.GetDbTransaction(), _db.ShardingOption.CommandTimeout);
            DataTable table = new DataTable();

            table.Load(reader);

            return table;
        }
        public override async Task<DataSet> GetDataSetWithSqlAsync(string sql, params (string paramterName, object value)[] parameters)
        {
            DbProviderFactory dbProviderFactory = _provider.DbProviderFactory;
            DbConnection conn = _db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = sql;
                cmd.CommandTimeout = _db.ShardingOption.CommandTimeout;
                if (parameters != null && parameters.Count() > 0)
                    cmd.Parameters.AddRange(CreateDbParamters(parameters).ToArray());

                DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();

                adapter.Fill(ds);
                cmd.Parameters.Clear();
                return ds;
            }
        }
        public override async Task<List<T>> GetListBySqlAsync<T>(string sql, params (string paramterName, object value)[] parameters)
        {
            var conn = _db.Database.GetDbConnection();

            return (await conn.QueryAsync<T>(sql, CreateDynamicParameters(parameters), _transaction?.GetDbTransaction(), _db.ShardingOption.CommandTimeout)).ToList();
        }

        #endregion

        #region 执行Sql语句

        public override async Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
#if EFCORE3||EFCORE5
            return await _db.Database.ExecuteSqlRawAsync(sql, CreateDbParamters(parameters).ToArray());
#endif

#if EFCORE2
            return await _db.Database.ExecuteSqlCommandAsync(sql, CreateDbParamters(parameters).ToArray());
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
