using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Sharding.Tests.Sharding
{
    [TestClass]
    public class PostgreSQLShardingDbAccessorTest : BaseTest
    {
        protected virtual IShardingDbAccessor _db { get; } = DbFactory.GetShardingDbAccessor("postgres"); //ServiceProvider.GetService<IShardingDbAccessor>();

        protected static SqlDefaultTestModel _defaultData { get; } = new SqlDefaultTestModel
        {
            
        };

        [TestMethod]
        public void CountTest()
        {
            _db.Insert(_defaultData);
            var theData = _db.GetIShardingQueryable<SqlDefaultTestModel>().Count();
            Assert.AreEqual(1, theData);
        }

        protected override void Clear()
        {
            //_db.DeleteAll<SqlDefaultTestModel>();
        }
    }
}
