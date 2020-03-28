using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace EFCore.Sharding.MySql
{
    public class MySqlProvider : AbstractProvider
    {
        public override DbProviderFactory DbProviderFactory => MySqlClientFactory.Instance;

        public override ModelBuilder GetModelBuilder() => new ModelBuilder(MySqlConventionSetBuilder.Build());

        public override IRepository GetRepository(string conString) => new MySqlRepository(conString);

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            dbContextOptionsBuilder.UseMySql(dbConnection);
        }
    }
}
