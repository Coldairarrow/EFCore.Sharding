using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using System;
using System.Data.Common;

namespace EFCore.Sharding.MySql
{
    internal class MySqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory
            => MySqlConnectorFactory.Instance;
        public override ModelBuilder GetModelBuilder() => new ModelBuilder(MySqlConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new MySqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = x => x.UseNetTopologySuite();

            dbContextOptionsBuilder.UseMySql(dbConnection, MySqlServerVersion.LatestSupportedServerVersion, mySqlOptionsAction);
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        }
    }
}
