using System;

namespace EFCore.Sharding.DataAnnotations
{
    /// <summary>
    /// 主键设置,支持联合主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KeysAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyNames">主键字段</param>
        public KeysAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }

        /// <summary>
        /// 主键字段
        /// </summary>
        public string[] PropertyNames { get; set; }
    }
}
