using EFCore.Sharding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EFCore.Sharding.Tests
{
    [SqlDefault("now()", "ModifiedOn")]
    [Table("sql_default_test")]
    public class SqlDefaultTestModel
    {
        public int Id { get; set; }

        public DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
}
