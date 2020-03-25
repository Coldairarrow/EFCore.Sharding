using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Performance
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
        [Key]
        [StringLength(50)]
        public String Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [StringLength(255)]
        public String UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(255)]
        public String UserName { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public Int32? Age { get; set; }

    }
}