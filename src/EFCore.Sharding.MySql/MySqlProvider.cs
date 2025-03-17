using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using System.Data.Common;

namespace EFCore.Sharding.MySql
{
    internal class MySqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory
            => MySqlConnectorFactory.Instance;
        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder(MySqlConventionSetBuilder.Build());
        }

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext)
        {
            return new MySqlDbAccessor(baseDbContext);
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            static void mySqlOptionsAction(MySqlDbContextOptionsBuilder x)
            {
                _ = x.UseNetTopologySuite();
            }

            _ = dbContextOptionsBuilder.UseMySql(dbConnection, MySqlServerVersion.LatestSupportedServerVersion, mySqlOptionsAction);
            _ = dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        }
    }
}
