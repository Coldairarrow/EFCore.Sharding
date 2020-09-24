using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Demo.DbMigrator
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
