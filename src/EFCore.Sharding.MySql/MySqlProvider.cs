using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace EFCore.Sharding.MySql
{
    internal class MySqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => MySqlClientFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(MySqlConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new MySqlDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseMySql(dbConnection);
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        }
    }
}
