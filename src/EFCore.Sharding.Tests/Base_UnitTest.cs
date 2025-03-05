using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.Sharding.Tests
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
        public string Id { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public int? Age { get; set; }
    }
}