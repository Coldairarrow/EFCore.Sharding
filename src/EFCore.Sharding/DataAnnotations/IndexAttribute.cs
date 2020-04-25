using System;

namespace EFCore.Sharding.DataAnnotations
{
    /// <summary>
    /// 索引设置,可以设置多个索引
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isUnique">是否为唯一索引</param>
        /// <param name="propertyNames">索引字段</param>
        public IndexAttribute(bool isUnique, params string[] propertyNames)
        {
            if (propertyNames.Length == 0)
                throw new Exception("索引字段不能为空");

            PropertyNames = propertyNames;
            IsUnique = isUnique;
        }

        /// <summary>
        /// 是否为唯一索引
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// 索引字段
        /// </summary>
        public string[] PropertyNames { get; set; }
    }
}
