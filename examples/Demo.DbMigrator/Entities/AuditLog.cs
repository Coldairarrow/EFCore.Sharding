using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.DbMigrator.Entities
{
    [Table("AuditLog")]
    class AuditLog
    {
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string Content { get; set; }
    }
}
