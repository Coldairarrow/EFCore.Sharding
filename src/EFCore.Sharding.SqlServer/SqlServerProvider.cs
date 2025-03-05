using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data.Common;

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder(SqlServerConventionSetBuilder.Build());
        }

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext)
        {
            return new SqlServerDbAccessor(baseDbContext);
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            _ = dbContextOptionsBuilder.UseSqlServer(dbConnection, x =>
            {
                _ = x.UseNetTopologySuite();
            });
            _ = dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator>();
        }
    }
}
