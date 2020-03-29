using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options)
            : base(options)
        {

        }

        public IQueryable GetIQueryable(Type entityType)
        {
            var dbSet = this.GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(this, null);
            var resQ = typeof(EntityFrameworkQueryableExtensions).GetMethod("AsNoTracking").MakeGenericMethod(entityType).Invoke(null, new object[] { dbSet });

            return resQ as IQueryable;
        }

        public void Detach()
        {
            ChangeTracker.Entries().ToList().ForEach(aEntry =>
            {
                if (aEntry.State != EntityState.Detached)
                    aEntry.State = EntityState.Detached;
            });
        }

        public override int SaveChanges()
        {
            int count = base.SaveChanges();
            Detach();

            return count;
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int count = await base.SaveChangesAsync(cancellationToken);
            Detach();

            return count;
        }
    }
}
