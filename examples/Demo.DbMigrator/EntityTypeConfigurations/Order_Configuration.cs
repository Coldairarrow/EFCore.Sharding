using Demo.DbMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMigrator
{
    internal class Order_Configuration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasIndex(x => x.OrderNum).IsUnique();
            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId);
        }
    }
}
