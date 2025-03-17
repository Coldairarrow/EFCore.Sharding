using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerDbAccessor : GenericDbAccessor, IDbAccessor
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
            using SqlBulkCopy bulkCopy = GetSqlBulkCopy();
            bulkCopy.BulkCopyTimeout = 0;

            bulkCopy.BatchSize = entities.Count;
            if (tableName.IsNullOrEmpty())
            {
                TableAttribute tableAttribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
                tableName = tableAttribute.Name;
            }
            bulkCopy.DestinationTableName = tableName;

            DataTable table = new();
            List<System.Reflection.PropertyInfo> props = typeof(T).GetProperties().Where(x => x.GetSetMethod() != null).ToList();

            foreach (System.Reflection.PropertyInfo propertyInfo in props)
            {
                string destinationColumn = string.Empty;

                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is not ColumnAttribute columnAttribute)
                    {
                        continue;
                    }

                    destinationColumn = columnAttribute.Name;

                    break;
                }

                _ = bulkCopy.ColumnMappings.Add(propertyInfo.Name,
                    string.IsNullOrEmpty(destinationColumn) ? propertyInfo.Name : destinationColumn);

                _ = table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
            }

            object[] values = new object[props.Count];

            foreach (T item in entities)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (props[i].GetSetMethod() == null)
                    {
                        continue;
                    }

                    values[i] = props[i].GetValue(item);
                }
                _ = table.Rows.Add(values);
            }

            bulkCopy.WriteToServer(table);
        }

        protected override string GetSchema(string schema)
        {
            return schema.IsNullOrEmpty() ? "dbo" : schema;
        }

        private SqlBulkCopy GetSqlBulkCopy()
        {
            SqlBulkCopy defaultSqlCopy = new(ConnectionString);

            return !_openedTransaction
                ? defaultSqlCopy
                : _db.Database.CurrentTransaction.GetDbTransaction() is not SqlTransaction sqlTransaction ?
                defaultSqlCopy :
                new SqlBulkCopy(sqlTransaction.Connection, SqlBulkCopyOptions.Default, sqlTransaction);
        }
    }
}
