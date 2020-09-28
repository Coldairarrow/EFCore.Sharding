using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.DbMigrator
{
    [Table(nameof(OrderItem))]
    public class OrderItem
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
    }
}
