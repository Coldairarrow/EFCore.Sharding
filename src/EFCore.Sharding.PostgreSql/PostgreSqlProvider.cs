using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using System.Data.Common;

namespace EFCore.Sharding.PostgreSql
{
    internal class PostgreSqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder(NpgsqlConventionSetBuilder.Build());
        }

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext)
        {
            return new PostgreSqlDbAccessor(baseDbContext);
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            _ = dbContextOptionsBuilder.UseNpgsql(dbConnection, x => x.UseNetTopologySuite());
            _ = dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingPostgreSqlMigrationsSqlGenerator>();
        }
    }
}
