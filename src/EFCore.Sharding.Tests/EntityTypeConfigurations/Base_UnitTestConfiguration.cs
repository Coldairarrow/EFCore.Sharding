using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Sharding.Tests
{
    internal class Base_UnitTestConfiguration : IEntityTypeConfiguration<Base_UnitTest>
    {
        [System.Obsolete]
        public void Configure(EntityTypeBuilder<Base_UnitTest> builder)
        {
            _ = builder.HasComment("单元测试表");
            _ = builder.Property(x => x.Id).HasComment("主键");
            _ = builder.Property(x => x.UserName).HasComment("用户名");
            _ = builder.Property(x => x.Age).HasComment("年龄");
            _ = builder.Property(x => x.CreateTime).HasComment("创建时间");
            _ = builder.Property(x => x.UserId).HasComment("用户Id");
        }
    }
}
