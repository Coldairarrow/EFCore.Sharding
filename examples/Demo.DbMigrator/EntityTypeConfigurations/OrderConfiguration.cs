using Demo.DbMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMigrator
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasIndex(x => x.OrderNum).IsUnique();
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}
