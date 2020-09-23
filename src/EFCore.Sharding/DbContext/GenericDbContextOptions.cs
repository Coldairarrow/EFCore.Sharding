using Microsoft.EntityFrameworkCore;
using System;

namespace EFCore.Sharding
{
    internal class GenericDbContextOptions
    {
        public DbContextOptions ContextOptions { get; set; }
        public string ConnectionString { get; set; }
        public DatabaseType DbType { get; set; }
        public string EntityNamespace { get; set; }
        public Type[] EntityTypes { get; set; }
        public string Suffix { get; set; }
    }
}
