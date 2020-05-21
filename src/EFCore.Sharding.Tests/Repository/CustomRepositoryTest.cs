using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class CustomRepositoryTest : DbRepositoryTest
    {
        protected override IRepository _db => ServiceProvider.GetService<ICustomRepository>();
    }
}
