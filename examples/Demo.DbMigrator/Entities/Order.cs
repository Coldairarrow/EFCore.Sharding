using Demo.DbMigrator.Entities;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
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
        [Key]
        public string Id { get; set; }

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

        public NpgsqlTsVector SearchVector { get; private set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderTypes OrderType { get; set; }

        public string[] Tags { get; set; }

        public int[] IntIds { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
