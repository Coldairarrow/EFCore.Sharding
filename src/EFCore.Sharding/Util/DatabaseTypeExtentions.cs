using System;

namespace EFCore.Sharding
{
    internal static class DatabaseTypeExtentions
    {
        public static string GetDefaultString(this DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseType.SqlServer => "Data Source=.;Initial Catalog=EFCoreSharding;Integrated Security=True;Pooling=true;",
                DatabaseType.PostgreSql => "Server=127.0.0.1;Port=5432;Database=EFCoreSharding;User Id=postgres;Password=postgres;",
                DatabaseType.MySql => "server=127.0.0.1;user id=root;password=root;persistsecurityinfo=True;database=EFCoreSharding;SslMode=none",
                DatabaseType.Oracle => "Data Source=127.0.0.1/ORCL;User ID=EFCORESHARDING;Password=123456;Connect Timeout=3",
                DatabaseType.SQLite => "Data Source=EFCoreSharding.db",
                _ => throw new Exception($"DatabaseType:{dbType} 无效"),
            };
        }
    }
}
