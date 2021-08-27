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

        public string[] Tags { get; set; }

        public string[] Tags2 { get; set; }
    }
}
