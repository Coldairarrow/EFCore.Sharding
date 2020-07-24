using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerDbAccessor : AbstractDbAccessor, IDbAccessor
    {
        public SqlServerDbAccessor(GenericDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"[{name}]";
        }

        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            using (var bulkCopy = GetSqlBulkCopy())
            {
                bulkCopy.BulkCopyTimeout = 0;

                bulkCopy.BatchSize = entities.Count;
                if (tableName.IsNullOrEmpty())
                {
                    var tableAttribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
                    tableName = tableAttribute.Name;
                }
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = typeof(T).GetProperties().Where(x => x.GetSetMethod() != null).ToList();

                foreach (var propertyInfo in props)
                {
                    var destinationColumn = string.Empty;

                    var attributes = propertyInfo.GetCustomAttributes(false);
                    foreach (var attribute in attributes)
                    {
                        if (!(attribute is ColumnAttribute columnAttribute)) continue;

                        destinationColumn = columnAttribute.Name;

                        break;
                    }

                    bulkCopy.ColumnMappings.Add(propertyInfo.Name,
                        string.IsNullOrEmpty(destinationColumn) ? propertyInfo.Name : destinationColumn);

                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Count];

                foreach (var item in entities)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (props[i].GetSetMethod() == null) continue;

                        values[i] = props[i].GetValue(item);
                    }
                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }

        protected override string GetSchema(string schema)
        {
            if (schema.IsNullOrEmpty())
                return "dbo";
            else
                return schema;
        }

        private SqlBulkCopy GetSqlBulkCopy()
        {
            var defaultSqlCopy = new SqlBulkCopy(ConnectionString);

            if (!_openedTransaction) return defaultSqlCopy;

            return !(_db.Database.CurrentTransaction.GetDbTransaction() is SqlTransaction sqlTransaction) ?
                defaultSqlCopy :
                new SqlBulkCopy(sqlTransaction.Connection, SqlBulkCopyOptions.Default, sqlTransaction);
        }
    }
}
