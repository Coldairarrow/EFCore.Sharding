using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Sharding.DataAnnotations
{
    /// <summary>
    /// Sql默认值
    /// y
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SqlDefaultAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="propertyNames"></param>
        public SqlDefaultAttribute(string sql, params string[] propertyNames)
        {
            this.Sql = sql;
            this.PropertyNames = propertyNames;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// 索引字段
        /// </summary>
        public string[] PropertyNames { get; set; }
    }
}
