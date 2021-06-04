using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data.Common;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

#if EFCORE3
using MySql.Data.MySqlClient;
#endif
#if EFCORE6
using MySqlConnector;
#endif

namespace EFCore.Sharding.MySql
{
    internal class MySqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory
#if EFCORE3
            => MySqlClientFactory.Instance;
#endif
#if EFCORE6
            => MySqlConnectorFactory.Instance;
#endif
        public override ModelBuilder GetModelBuilder() => new ModelBuilder(MySqlConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new MySqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = x => x.UseNetTopologySuite();
#if EFCORE3
            dbContextOptionsBuilder.UseMySql(dbConnection, mySqlOptionsAction);
#endif
#if EFCORE6
            dbContextOptionsBuilder.UseMySql(dbConnection, MySqlServerVersion.LatestSupportedServerVersion, mySqlOptionsAction);
#endif
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        }
    }
}
