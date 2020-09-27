using EFCore.Sharding;

namespace Demo.DbMigrator
{
    public class CustomContext : GenericDbContext
    {
        public CustomContext(GenericDbContext dbContext) 
            : base(dbContext)
        {

        }
    }
}
