namespace EFCore.Sharding
{
    internal class PhysicDb
    {
        public string GroupName { get; set; }
        public ReadWriteType OpType { get; set; }
        public string ConString { get; set; }
    }
}
