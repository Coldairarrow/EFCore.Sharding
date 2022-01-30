using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.Sharding
{
    internal class GenericModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => context is GenericDbContext dynamicContext
                ? (context.GetType(), $"{dynamicContext.Paramter.EntityNamespace}:{dynamicContext.Paramter.Suffix}", designTime)
                : (object)context.GetType();

        public object Create(DbContext context)
            => Create(context, false);
    }
}
