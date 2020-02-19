namespace EFCore.Sharding
{
    internal class AbsDb
    {
        public string Name { get; set; }
        public DatabaseType DbType { get; set; }
    }
}
