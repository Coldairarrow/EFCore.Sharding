using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using System.Data.Common;

namespace EFCore.Sharding.PostgreSql
{
    public class PostgreSqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(NpgsqlConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(BaseDbContext baseDbContext) => new PostgreSqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseNpgsql(dbConnection);
        }
    }
}
