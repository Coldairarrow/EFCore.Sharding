using System;

namespace EFCore.Sharding
{
    internal class PhysicTable
    {
        public Type EntityType { get; set; }
        public string DataSourceName { get; set; }
        public string Suffix { get; set; }
    }
}
