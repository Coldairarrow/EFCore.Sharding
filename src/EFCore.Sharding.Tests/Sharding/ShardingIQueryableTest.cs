using EFCore.Sharding.Tests.Util;
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
        private IShardingDbAccessor _db { get => ServiceProvider.GetService<IShardingDbAccessor>(); }
        protected override void Clear()
        {
            _db.DeleteAll<Base_UnitTest>();
        }

        [TestMethod]
        public void Where()
        {
            _db.Insert(_dataList);
            var local = _dataList.Where(x => x.Age > 50).Count();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.Age > 50).Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Where_dynamic()
        {
            _db.Insert(_dataList);
            var local = _dataList.Where(x => x.Age > 50).Count();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Where("Age > 50").Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Skip()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).Skip(1).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).Skip(1).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Take()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).Take(1).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).Take(1).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderBy()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderBy_dynamic()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy("Id asc").ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void OrderByDescending()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderByDescending(x => x.Id).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderByDescending(x => x.Id).ToList().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Count()
        {
            _db.Insert(_dataList);
            var local = _dataList.Count;
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task CountAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Count;
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().CountAsync();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void ToList()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().ToList().OrderBy(x => x.Id).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task ToListAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).ToJson();
            var db = (await _db.GetIShardingQueryable<Base_UnitTest>().ToListAsync()).OrderBy(x => x.Id).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).FirstOrDefault().ToJson();
            var db = _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).FirstOrDefault().ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task FirstOrDefaultAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.OrderBy(x => x.Id).FirstOrDefault().ToJson();
            var db = (await _db.GetIShardingQueryable<Base_UnitTest>().OrderBy(x => x.Id).FirstOrDefaultAsync()).ToJson();
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Any()
        {
            _db.Insert(_dataList);
            var local = _dataList.Any(x => x.Age == 99);
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Any(x => x.Age == 99);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task AnyAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Any(x => x.Age == 99);
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().AnyAsync(x => x.Age == 99);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Max()
        {
            _db.Insert(_dataList);
            var local = _dataList.Max(x => x.Age);
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Max(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task MaxAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Max(x => x.Age);
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().MaxAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Min()
        {
            _db.Insert(_dataList);
            var local = _dataList.Min(x => x.Age);
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Min(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task MinAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Min(x => x.Age);
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().MinAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Average()
        {
            _db.Insert(_dataList);
            var local = _dataList.Average(x => x.Age);
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Average(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task AverageAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Average(x => x.Age);
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().AverageAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public void Sum()
        {
            _db.Insert(_dataList);
            var local = _dataList.Sum(x => x.Age);
            var db = _db.GetIShardingQueryable<Base_UnitTest>().Sum(x => x.Age);
            Assert.AreEqual(local, db);
        }

        [TestMethod]
        public async Task SumAsync()
        {
            _db.Insert(_dataList);
            var local = _dataList.Sum(x => x.Age);
            var db = await _db.GetIShardingQueryable<Base_UnitTest>().SumAsync(x => x.Age);
            Assert.AreEqual(local, db);
        }
    }
}
