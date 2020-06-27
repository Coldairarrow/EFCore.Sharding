﻿using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Metadata.Conventions;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace EFCore.Sharding.Oracle
{
    public class OracleProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => OracleClientFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(OracleConventionSetBuilder.Build());

        public override IRepository GetRepository(BaseDbContext baseDbContext) => new OracleRepository(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseOracle(dbConnection, x => x.UseOracleSQLCompatibility("11"));
        }
    }
}
