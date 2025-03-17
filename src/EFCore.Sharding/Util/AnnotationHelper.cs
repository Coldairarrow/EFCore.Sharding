using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace EFCore.Sharding
{
    internal static class AnnotationHelper
    {
        public static string GetDbTableName(Type type)
        {
            //表名
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute != null ? tableAttribute.Name : type.Name;
            return tableName;
        }

        public static string GetDbSchemaName(Type type)
        {
            //表名
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();

            return tableAttribute.Schema;
        }
    }
}
