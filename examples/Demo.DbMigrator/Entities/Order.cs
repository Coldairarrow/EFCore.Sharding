using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.DbMigrator
{
    [Table(nameof(Order))]
    public class Order
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [StringLength(50)]
        public string OrderNum { get; set; }

        /// <summary>
        /// 订单名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int Count { get; set; }
    }
}
