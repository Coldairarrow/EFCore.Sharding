using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Common
{
    /// <summary>
    /// 单元测试表
    /// </summary>
    [Table("Base_UnitTest")]
    public class Base_UnitTest
    {
        /// <summary>
        /// 代理主键
        /// </summary>
        [Key, StringLength(50)]
        public String Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public Int32? Age { get; set; }
    }
}