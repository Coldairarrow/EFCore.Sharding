using EFCore.Sharding.Util;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EFCore.Sharding
{
    internal static class ShardingHelper
    {
        /// <summary>
        /// 映射物理表
        /// </summary>
        /// <param name="absTable">抽象表类型</param>
        /// <param name="targetTableName">目标物理表名</param>
        /// <returns></returns>
        public static Type MapTable(Type absTable, string targetTableName)
        {
            var config = TypeBuilderHelper.GetConfig(absTable);

            config.AssemblyName = "EFCore.Sharding";

            var theTableAttribute = config.Attributes
                .Where(x => x.Attribute == typeof(TableAttribute))
                .FirstOrDefault();
            if (theTableAttribute != null)
            {
                theTableAttribute.ConstructorArgs[0] = targetTableName;
            }

            config.Attributes.RemoveAll(x => x.Attribute == typeof(TableAttribute));
            config.FullName = $"EFCore.Sharding.{targetTableName}";

            return TypeBuilderHelper.BuildType(config);
        }
    }
}
