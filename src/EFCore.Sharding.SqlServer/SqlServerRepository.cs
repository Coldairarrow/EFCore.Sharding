using System;
using EFCore.Sharding.Util;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerRepository : DbRepository, IRepository
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        public SqlServerRepository(string conStr)
            : base(conStr, DatabaseType.SqlServer)
        {
        }

        #endregion

        #region 私有成员

        protected override string FormatFieldName(string name)
        {
            return $"[{name}]";
        }

        #endregion

        #region 插入数据

        /// <summary>
        /// 使用Bulk批量插入数据（适合大数据量，速度非常快）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">数据</param>
        public override void BulkInsert<T>(List<T> entities)
        {
            using (var bulkCopy = new SqlBulkCopy(ConnectionString))
            {
                bulkCopy.BatchSize = entities.Count;
                var tableAttribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
                var tableName = tableAttribute.Name;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = typeof(T).GetProperties().Where( x => x.GetSetMethod() != null).ToList();

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

        #endregion
    }
}
