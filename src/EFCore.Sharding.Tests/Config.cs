namespace EFCore.Sharding.Tests
{
    public static class Config
    {
        public const string CONSTRING1 = "Data Source=127.0.0.1;Initial Catalog=EFCore.Sharding1;Integrated Security=True";
        public const string CONSTRING2 = "Data Source=127.0.0.1;Initial Catalog=EFCore.Sharding2;Integrated Security=True";
        public const string SQLITE1 = "DataSource=db1.db";
        public const string SQLITE2 = "DataSource=db2.db";
    }
}
