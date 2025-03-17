using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace EFCore.Sharding.MySql
{
    internal class MySqlDbAccessor : GenericDbAccessor, IDbAccessor
    {
        public MySqlDbAccessor(GenericDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"`{name}`";
        }

        public override void BulkInsert<T>(List<T> entities, string tableName)
        {
            DataTable dt = entities.ToDataTable();
            using MySqlConnection conn = new();
            conn.ConnectionString = ConnectionString;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            if (tableName.IsNullOrEmpty())
            {
                object tableAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
                tableName = tableAttribute != null ? ((TableAttribute)tableAttribute).Name : typeof(T).Name;
            }

            int insertCount = 0;
            string tmpPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString() + "_" + Guid.NewGuid().ToString() + ".tmp");
            string csv = dt.ToCsvStr();
            File.WriteAllText(tmpPath, csv, Encoding.UTF8);

            using (MySqlTransaction tran = conn.BeginTransaction())
            {
                MySqlBulkLoader bulk = new(conn)
                {
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n",
                    FileName = tmpPath,
                    NumberOfLinesToSkip = 0,
                    TableName = tableName,
                };
                try
                {
                    bulk.Columns.AddRange(dt.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList());
                    insertCount = bulk.Load();
                    tran.Commit();
                }
                catch (MySqlException ex)
                {
                    tran?.Rollback();

                    throw ex;
                }
            }
            File.Delete(tmpPath);
        }

        protected override string GetSchema(string schema)
        {
            return null;
        }
    }
}
