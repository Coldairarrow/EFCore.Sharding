using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.DbMigrator
{
    [Table(nameof(Order))]
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
