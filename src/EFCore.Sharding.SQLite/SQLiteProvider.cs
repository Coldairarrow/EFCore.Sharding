using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data.Common;

namespace EFCore.Sharding.SQLite
{
    internal class SQLiteProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => SqliteFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(SqliteConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new SQLiteDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseSqlite(dbConnection);
        }
    }
}
