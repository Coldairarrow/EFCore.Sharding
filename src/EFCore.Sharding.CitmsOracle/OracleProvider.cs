using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace EFCore.Sharding.Oracle
{
    internal class OracleProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => OracleClientFactory.Instance;

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new OracleDbAccessor(baseDbContext);

        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder();
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseOracle(dbConnection);
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingOracleMigrationsSqlGenerator>();
        }
    }
}
