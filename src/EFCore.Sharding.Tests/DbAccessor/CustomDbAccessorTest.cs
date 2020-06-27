using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class CustomDbAccessorTest : DbAccessorTest
    {
        protected override IDbAccessor _db => ServiceProvider.GetService<ICustomDbAccessor>();
    }
}
