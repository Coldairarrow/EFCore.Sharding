using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using System.Data.Common;

namespace EFCore.Sharding.PostgreSql
{
    internal class PostgreSqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(NpgsqlConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new PostgreSqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection, GenericDbContextOptions options)
        {
            dbContextOptionsBuilder.UseNpgsql(dbConnection, config => config.CommandTimeout(options.ShardingConfig?.CommandTimeout ?? 30));
        }
    }
}
