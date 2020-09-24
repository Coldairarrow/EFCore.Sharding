using Demo.DbMigrator;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DbMigrator
{
    public class DbContext1 : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Data Source=.;Initial Catalog=DbMigrator;Integrated Security=True;";

            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderItem>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
