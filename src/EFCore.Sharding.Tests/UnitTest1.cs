using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var db = DbFactory.GetRepository("Data Source=.;Initial Catalog=Colder.Admin.AntdVue;Integrated Security=True;Pooling=true;", DatabaseType.SqlServer);
        }
    }
}
