using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace EFCore.Sharding.Tests.Sharding
{
    [TestClass]
    public class ShardingHelperTest
    {
        private readonly static string _table1 = new string[] { "202001" }.ToJson();
        private readonly static string _table2 = new string[] { "202002" }.ToJson();
        private readonly static string _table3 = new string[] { "202003" }.ToJson();
        private readonly static string _table123 = new string[] { "202001", "202002", "202003" }.ToJson();
        private readonly static string _table12 = new string[] { "202001", "202002" }.ToJson();
        private readonly static string _table23 = new string[] { "202002", "202003" }.ToJson();

        [TestMethod]
        public void FilterTable()
        {
            using var scop = Startup.ServiceScopeFactory.CreateScope();

            var db = scop.ServiceProvider.GetService<IDbAccessor>();

            var q = db.GetIQueryable<Base_UnitTest>();
            ShardingRule rule = new ShardingRule
            {
                EntityType = typeof(Base_UnitTest),
                ExpandByDateMode = ExpandByDateMode.PerMonth,
                ShardingField = nameof(Base_UnitTest.CreateTime),
                ShardingType = ShardingType.Date
            };
            List<string> tableSuffixs = new List<string>() { "202001", "202002", "202003" };
            DateTime time0 = DateTime.Parse("2019-12-01");
            DateTime time1 = DateTime.Parse("2020-01-01");
            DateTime time2 = DateTime.Parse("2020-02-01");
            DateTime time3 = DateTime.Parse("2020-03-01");
            DateTime time4 = DateTime.Parse("2020-04-01");
            List<string> res;
            //=
            res = GetFilterTable(x => x.CreateTime == time1);
            Assert.AreEqual(res.ToJson(), _table1);

            res = GetFilterTable(x => x.CreateTime == time0);
            Assert.AreEqual(res.Count, 0);

            res = GetFilterTable(x => x.CreateTime == time4);
            Assert.AreEqual(res.Count, 0);
            //!=
            res = GetFilterTable(x => x.CreateTime != time1);
            Assert.AreEqual(res.ToJson(), _table23);

            res = GetFilterTable(x => x.CreateTime != time0);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime != time4);
            Assert.AreEqual(res.ToJson(), _table123);
            //>
            res = GetFilterTable(x => x.CreateTime > time0);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime > time2);
            Assert.AreEqual(res.ToJson(), _table23);

            res = GetFilterTable(x => x.CreateTime > time3);
            Assert.AreEqual(res.Count, 1);

            res = GetFilterTable(x => x.CreateTime > time4);
            Assert.AreEqual(res.Count, 0);
            //>=
            res = GetFilterTable(x => x.CreateTime > time0);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime > time1);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime > time2);
            Assert.AreEqual(res.ToJson(), _table23);

            res = GetFilterTable(x => x.CreateTime > time3);
            Assert.AreEqual(res.Count, 1);

            res = GetFilterTable(x => x.CreateTime > time4);
            Assert.AreEqual(res.Count, 0);
            //<
            res = GetFilterTable(x => x.CreateTime < time0);
            Assert.AreEqual(res.Count, 0);

            res = GetFilterTable(x => x.CreateTime < time1);
            Assert.AreEqual(res.ToJson(), _table1);

            res = GetFilterTable(x => x.CreateTime < time2);
            Assert.AreEqual(res.ToJson(), _table12);

            res = GetFilterTable(x => x.CreateTime < time3);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime < time4);
            Assert.AreEqual(res.ToJson(), _table123);
            //<=
            res = GetFilterTable(x => x.CreateTime < time0);
            Assert.AreEqual(res.Count, 0);

            res = GetFilterTable(x => x.CreateTime < time1);
            Assert.AreEqual(res.ToJson(), _table1);

            res = GetFilterTable(x => x.CreateTime < time2);
            Assert.AreEqual(res.ToJson(), _table12);

            res = GetFilterTable(x => x.CreateTime < time3);
            Assert.AreEqual(res.ToJson(), _table123);

            res = GetFilterTable(x => x.CreateTime < time4);

            Assert.AreEqual(res.ToJson(), _table123); List<string> GetFilterTable(Expression<Func<Base_UnitTest, bool>> theWhere)
            {
                return ShardingHelper.FilterTable(db.GetIQueryable<Base_UnitTest>().Where(theWhere), tableSuffixs, rule);
            }
        }
    }
}
