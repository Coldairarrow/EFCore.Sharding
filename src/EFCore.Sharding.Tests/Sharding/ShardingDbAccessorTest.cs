using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class ShardingDbAccessorTest : BaseTest
    {
        private readonly IShardingDbAccessor _db;
        public ShardingDbAccessorTest()
        {
            _db = ServiceProvider.GetService<IShardingDbAccessor>();
        }

        protected override void Clear()
        {
            _ = _db.DeleteAll<Base_UnitTest>();
        }

        [TestMethod]
        public void Insert_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest theData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public async Task InsertAsync_single()
        {
            _ = await _db.InsertAsync(_newData);
            Base_UnitTest theData = await _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefaultAsync();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public void DeleteAll_generic()
        {
            _ = _db.Insert(_insertList);
            _ = _db.DeleteAll<Base_UnitTest>();
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAllAsync_generic()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAllAsync<Base_UnitTest>();
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_single()
        {
            _ = _db.Insert(_newData);
            _ = _db.Delete(_newData);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_single()
        {
            _ = _db.Insert(_newData);
            _ = await _db.DeleteAsync(_newData);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_multiple()
        {
            _ = _db.Insert(_insertList);
            _ = _db.Delete(_insertList);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_multiple()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAsync(_insertList);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_where()
        {
            _ = _db.Insert(_insertList);
            _ = _db.Delete<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task DeleteAsync_where()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAsync<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task Delete_Sql()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteSqlAsync<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Update_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _ = _db.Update(updateData);
            Base_UnitTest dbUpdateData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());
        }

        [TestMethod]
        public async Task UpdateAsync_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _ = await _db.UpdateAsync(updateData);
            Base_UnitTest dbUpdateData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());
        }

        [TestMethod]
        public void Update_multiple()
        {
            _ = _db.Insert(_insertList);
            List<Base_UnitTest> updateList = _insertList.DeepClone();
            updateList[0].UserId = "Admin3";
            updateList[1].UserId = "Admin4";
            _ = _db.Update(updateList);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task UpdateAsync_multiple()
        {
            _ = _db.Insert(_insertList);
            List<Base_UnitTest> updateList = _insertList.DeepClone();
            updateList[0].UserId = "Admin3";
            updateList[1].UserId = "Admin4";
            _ = await _db.UpdateAsync(updateList);
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void UpdateAny_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest newUpdateData = _newData.DeepClone();
            newUpdateData.UserName = "普通管理员";
            newUpdateData.UserId = "xiaoming";
            newUpdateData.Age = 100;
            _ = _db.Update(newUpdateData, ["UserName", "Age"]);
            Base_UnitTest dbSingleData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            newUpdateData.UserId = "Admin";
            Assert.AreEqual(newUpdateData.ToJson(), dbSingleData.ToJson());
        }

        [TestMethod]
        public async Task UpdateAnyAsync_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest newUpdateData = _newData.DeepClone();
            newUpdateData.UserName = "普通管理员";
            newUpdateData.UserId = "xiaoming";
            newUpdateData.Age = 100;
            _ = await _db.UpdateAsync(newUpdateData, ["UserName", "Age"]);
            Base_UnitTest dbSingleData = _db.GetIShardingQueryable<Base_UnitTest>().FirstOrDefault();
            newUpdateData.UserId = "Admin";
            Assert.AreEqual(newUpdateData.ToJson(), dbSingleData.ToJson());
        }

        [TestMethod]
        public void UpdateAny_multiple()
        {
            _ = _db.Insert(_insertList);
            List<Base_UnitTest> newList1 = _insertList.DeepClone();
            List<Base_UnitTest> newList2 = _insertList.DeepClone();
            newList1.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserId = "Test";
                aData.UserName = "测试";
            });
            newList2.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserName = "测试";
            });

            _ = _db.Update(newList1, ["UserName", "Age"]);
            List<Base_UnitTest> dbData = _db.GetIShardingQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(newList2.OrderBy(x => x.Id).ToJson(), dbData.OrderBy(x => x.Id).ToJson());
        }

        [TestMethod]
        public async Task UpdateAnyAsync_multiple()
        {
            _ = _db.Insert(_insertList);
            List<Base_UnitTest> newList1 = _insertList.DeepClone();
            List<Base_UnitTest> newList2 = _insertList.DeepClone();
            newList1.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserId = "Test";
                aData.UserName = "测试";
            });
            newList2.ForEach(aData =>
            {
                aData.Age = 100;
                aData.UserName = "测试";
            });

            _ = await _db.UpdateAsync(newList1, ["UserName", "Age"]);
            List<Base_UnitTest> dbData = _db.GetIShardingQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(newList2.OrderBy(x => x.Id).ToJson(), dbData.OrderBy(x => x.Id).ToJson());
        }

        [TestMethod]
        public void UpdateWhere()
        {
            _ = _db.Insert(_newData);
            _ = _db.Update<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIShardingQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public async Task UpdateWhereAsync()
        {
            _ = _db.Insert(_newData);
            _ = await _db.UpdateAsync<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIShardingQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public void RunTransaction_fail()
        {
            bool succcess = _db.RunTransaction(() =>
            {
                _ = _db.Insert(_newData);
                Base_UnitTest newData2 = _newData.DeepClone();
                _ = _db.Insert(newData2);
            }).Success;
            Assert.IsFalse(succcess);
        }

        [TestMethod]
        public async Task RunTransactionAsync_fail()
        {
            bool succcess = (await _db.RunTransactionAsync(async () =>
            {
                _ = await _db.InsertAsync(_newData);
                Base_UnitTest newData2 = _newData.DeepClone();
                _ = await _db.InsertAsync(newData2);
            })).Success;
            Assert.IsFalse(succcess);
        }

        [TestMethod]
        public void RunTransaction_success()
        {
            bool succcess = _db.RunTransaction(() =>
            {
                Base_UnitTest newData = _newData.DeepClone();
                newData.Id = Guid.NewGuid().ToString();
                newData.UserId = Guid.NewGuid().ToString();
                newData.UserName = Guid.NewGuid().ToString();
                _ = _db.Insert(_newData);
                _ = _db.Insert(newData);
            }).Success;
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.IsTrue(succcess);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task RunTransactionAsync_success()
        {
            bool succcess = (await _db.RunTransactionAsync(async () =>
            {
                Base_UnitTest newData = _newData.DeepClone();
                newData.Id = Guid.NewGuid().ToString();
                newData.UserId = Guid.NewGuid().ToString();
                newData.UserName = Guid.NewGuid().ToString();
                _ = await _db.InsertAsync(_newData);
                _ = await _db.InsertAsync(newData);
            })).Success;
            int count = _db.GetIShardingQueryable<Base_UnitTest>().Count();
            Assert.IsTrue(succcess);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void RunTransaction_isolationLevel()
        {
            IShardingDbAccessor db1 = ServiceProvider.GetService<IShardingDbAccessor>();
            IShardingDbAccessor db2 = ServiceProvider.CreateScope().ServiceProvider.GetService<IShardingDbAccessor>();
            _ = db1.Insert(_newData);

            Base_UnitTest updateData = _newData.DeepClone();
            Task db2Task = new(() =>
            {
                updateData.UserName = Guid.NewGuid().ToString();
                _ = db2.Update(updateData);
            });

            (bool Success, Exception ex) = db1.RunTransaction(() =>
            {
                //db1读=>db2写(阻塞)=>db1读=>db1提交
                Base_UnitTest db1Data_1 = db1.GetIShardingQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();

                db2Task.Start();

                Base_UnitTest db1Data_2 = db1.GetIShardingQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
                Assert.AreEqual(db1Data_1.ToJson(), db1Data_2.ToJson());
            });
            db2Task.Wait();
            Base_UnitTest db1Data_3 = db1.GetIShardingQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), db1Data_3.ToJson());
        }
    }
}
