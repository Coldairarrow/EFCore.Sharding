using EFCore.Sharding;

namespace DbMigrator
{
    public class CustomContext : GenericDbContext
    {
        public CustomContext(GenericDbContext dbContext) 
            : base(dbContext)
        {

        }
    }
}
