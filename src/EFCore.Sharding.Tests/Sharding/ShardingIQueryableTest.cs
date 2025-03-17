using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class ShardingIQueryableTest : BaseTest
    {
        private readonly IShardingDbAccessor _db;
        public ShardingIQueryableTest()
        {
            _db = ServiceProvider.GetService<IShardingDbAccessor>();
        }

        protected override void Clear()
        {
            _ = _db.DeleteAll<Base_UnitTest>();
        }

        [TestMethod]
        public void Where()
        {
            _ = _db.Insert(_dataList);
            int local = _dataList.Where(x => x.Age > 50).Count();
            int db = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.Age > 50).Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Where_dynamic()
        {
            _ = _db.Insert(_dataList);
            int local = _dataList.Where(x => x.Age > 50).Count();
            int db = _db.GetIShardingQueryable<Base_UnitTest>().Where("Age > 50").Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Skip()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).Skip(1).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).Skip(1).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Take()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).Take(1).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).Take(1).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderBy()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderBy_dynamic()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy("Id asc").ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderByDescending()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderByDescending(x => x.Id).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderByDescending(x => x.Id).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Count()
        {
            _ = _db.Insert(_dataList);
            int local = _dataList.Count;
            int db = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task CountAsync()
        {
            _ = _db.Insert(_dataList);
            int local = _dataList.Count;
            int db = await _db.GetIShardingQueryable<Base_UnitTest>().CountAsync();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void ToList()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().ToList().OrderBy(x => x.Id).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task ToListAsync()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).ToJson();
            string db = (await _db.GetIShardingQueryable<Base_UnitTest>().ToListAsync()).OrderBy(x => x.Id).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).FirstOrDefault().ToJson();
            string db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).FirstOrDefault().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task FirstOrDefaultAsync()
        {
            _ = _db.Insert(_dataList);
            string local = _dataList.OrderBy(x => x.Id).FirstOrDefault().ToJson();
            string db = (await _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).FirstOrDefaultAsync()).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Any()
        {
            _ = _db.Insert(_dataList);
            bool local = _dataList.Any(x => x.Age == 99);
            bool db = _db.GetIShardingQueryable<Base_UnitTest>().Any(x => x.Age == 99);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task AnyAsync()
        {
            _ = _db.Insert(_dataList);
            bool local = _dataList.Any(x => x.Age == 99);
            bool db = await _db.GetIShardingQueryable<Base_UnitTest>().AnyAsync(x => x.Age == 99);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Max()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Max(x => x.Age);
            int? db = _db.GetIShardingQueryable<Base_UnitTest>().Max(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task MaxAsync()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Max(x => x.Age);
            int? db = await _db.GetIShardingQueryable<Base_UnitTest>().MaxAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Min()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Min(x => x.Age);
            int? db = _db.GetIShardingQueryable<Base_UnitTest>().Min(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task MinAsync()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Min(x => x.Age);
            int? db = await _db.GetIShardingQueryable<Base_UnitTest>().MinAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Average()
        {
            _ = _db.Insert(_dataList);
            double? local = _dataList.Average(x => x.Age);
            double? db = _db.GetIShardingQueryable<Base_UnitTest>().Average(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task AverageAsync()
        {
            _ = _db.Insert(_dataList);
            double? local = _dataList.Average(x => x.Age);
            double? db = await _db.GetIShardingQueryable<Base_UnitTest>().AverageAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Sum()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Sum(x => x.Age);
            int? db = _db.GetIShardingQueryable<Base_UnitTest>().Sum(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task SumAsync()
        {
            _ = _db.Insert(_dataList);
            int? local = _dataList.Sum(x => x.Age);
            int? db = await _db.GetIShardingQueryable<Base_UnitTest>().SumAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Distinct()
        {
            _ = _db.Insert(_dataList);
            IOrderedEnumerable<string> local = _dataList.Select(x => x.UserName).Distinct().ToList().OrderBy(x => x);
            IOrderedEnumerable<string> db = _db.GetIShardingQueryable<Base_UnitTest>().Distinct(x => x.UserName).OrderBy(x => x);
            Assert.AreEqual(local.ToJson(), db.ToJson());
        }

        [TestMethod]
        public async Task DistinctAsync()
        {
            _ = _db.Insert(_dataList);
            IOrderedEnumerable<string> local = _dataList.Select(x => x.UserName).Distinct().ToList().OrderBy(x => x);
            IOrderedEnumerable<string> db = (await _db.GetIShardingQueryable<Base_UnitTest>().DistinctAsync(x => x.UserName)).OrderBy(x => x);
            Assert.AreEqual(local.ToJson(), db.ToJson());
        }
    }
}
