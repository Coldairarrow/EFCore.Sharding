using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data.Common;
using System.Data.SqlClient;

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(SqlServerConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new SqlServerDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseSqlServer(dbConnection);
        }
    }
}
