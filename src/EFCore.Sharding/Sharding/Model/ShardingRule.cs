using System;

namespace EFCore.Sharding
{
    internal class ShardingRule
    {
        public string AbsDb { get; set; }
        public string AbsTable { get; set; }
        public ShardingType ShardingType { get; set; }
        public string ShardingField { get; set; }
        public ExpandByDateMode? ExpandByDateMode { get; set; }
        public Func<object, string> FindTable { get; set; }
    }
}
