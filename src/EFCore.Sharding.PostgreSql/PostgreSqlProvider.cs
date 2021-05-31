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

#pragma warning disable EF1001 // Internal EF Core API usage.
        public override ModelBuilder GetModelBuilder() => new ModelBuilder(NpgsqlConventionSetBuilder.Build());
#pragma warning restore EF1001 // Internal EF Core API usage.

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new PostgreSqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseNpgsql(dbConnection, x => x.UseNetTopologySuite().EnableRetryOnFailure());
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingPostgreSqlMigrationsSqlGenerator>();
        }
    }
}
