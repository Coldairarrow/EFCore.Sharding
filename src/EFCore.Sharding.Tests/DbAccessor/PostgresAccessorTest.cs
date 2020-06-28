using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Sharding.Tests.DbAccessor
{
    [TestClass]
    public class PostgresAccessorTest : BaseTest
    {
        protected virtual IDbAccessor _db { get; } = ServiceProvider.GetService<IDbAccessor>();


        public void CountTest()
        {
            
        }

        protected override void Clear()
        {
            _db.DeleteAll<SqlDefaultTestModel>();
        }
    }
}
