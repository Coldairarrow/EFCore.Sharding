using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata.Conventions;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace EFCore.Sharding.Oracle
{
    internal class OracleProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => OracleClientFactory.Instance;

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new OracleDbAccessor(baseDbContext);

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(OracleConventionSetBuilder.Build());

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {

#if NET8_0
dbContextOptionsBuilder.UseOracle(dbConnection, x => x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19));

#else   
            dbContextOptionsBuilder.UseOracle(dbConnection, x => x.UseOracleSQLCompatibility("11"));
#endif            
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingOracleMigrationsSqlGenerator>();
        }
    }
}
