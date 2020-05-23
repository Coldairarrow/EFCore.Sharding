using EFCore.Sharding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Sharding.Tests
{
    [SqlDefault("now()", "ModifiedOn")]
    public class SqlDefaultTestModel
    {
        public int Id { get; set; }

        public DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
}
