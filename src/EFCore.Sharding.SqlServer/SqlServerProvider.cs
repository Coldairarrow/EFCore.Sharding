using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data.Common;

#if EFCORE3
using Microsoft.Data.SqlClient;
#elif EFCORE2
using System.Data.SqlClient;
#endif

namespace EFCore.Sharding.SqlServer
{
    internal class SqlServerProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(SqlServerConventionSetBuilder.Build());

        public override IDbAccessor GetDbAccessor(GenericDbContext baseDbContext) => new SqlServerDbAccessor(baseDbContext);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseSqlServer(dbConnection, x =>
            {
                x.UseNetTopologySuite();
#if EFCORE2
                x.UseRowNumberForPaging();
#endif
            });
        }
    }
}
