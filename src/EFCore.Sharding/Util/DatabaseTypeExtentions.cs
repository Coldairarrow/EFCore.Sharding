using System;

namespace EFCore.Sharding
{
    internal static class DatabaseTypeExtentions
    {
        public static string GetDefaultString(this DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    return "Data Source=.;Initial Catalog=EFCoreSharding;Integrated Security=True;Pooling=true;";
                case DatabaseType.PostgreSql:
                    return "Server=127.0.0.1;Port=5432;Database=EFCoreSharding;User Id=postgres;Password=postgres;";
                case DatabaseType.MySql:
                    return "server=127.0.0.1;user id=root;password=root;persistsecurityinfo=True;database=EFCoreSharding;SslMode=none";
                case DatabaseType.Oracle:
                    return "Data Source=127.0.0.1/ORCL;User ID=EFCORESHARDING;Password=123456;Connect Timeout=3";
                case DatabaseType.SQLite:
                    return "Data Source=EFCoreSharding.db";
                default:
                    throw new Exception($"DatabaseType:{dbType} 无效");
            }
        }
    }
}
