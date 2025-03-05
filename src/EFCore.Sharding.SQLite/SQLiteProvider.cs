using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data.Common;

namespace EFCore.Sharding.SQLite
{
    internal class SQLiteProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => SqliteFactory.Instance;

        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder(SqliteConventionSetBuilder.Build());
        }

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext)
        {
            return new SQLiteDbAccessor(baseDbContext);
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            _ = dbContextOptionsBuilder.UseSqlite(dbConnection, x => x.UseNetTopologySuite());
            _ = dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingSQLiteMigrationsSqlGenerator>();
        }
    }
}
