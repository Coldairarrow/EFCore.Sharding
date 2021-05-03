using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Sharding.Tests
{
    [TestClass]
    public class DbAccessorTest : BaseTest
    {
        protected IDbAccessor _db { get; set; }
        public DbAccessorTest()
        {
            _db = ServiceProvider.GetService<IDbAccessor>();
        }
        protected override void Clear()
        {
            _db.DeleteAll<Base_UnitTest>();
        }

        [TestMethod]
        public void Insert_single()
        {
            _db.Insert(_newData);
            var theData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public async Task InsertAsync_single()
        {
            await _db.InsertAsync(_newData);
            var theData = await _db.GetIQueryable<Base_UnitTest>().FirstOrDefaultAsync();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public void Insert_multiple()
        {
            _db.Insert(_insertList);
            var theList = _db.GetIQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(_insertList.OrderBy(X => X.Id).ToJson(), theList.OrderBy(X => X.Id).ToJson());
        }

        [TestMethod]
        public async Task InsertAsync_multiple()
        {
            await _db.InsertAsync(_insertList);
            var theList = await _db.GetIQueryable<Base_UnitTest>().ToListAsync();
            Assert.AreEqual(_insertList.OrderBy(X => X.Id).ToJson(), theList.OrderBy(X => X.Id).ToJson());
        }

        [TestMethod]
        public void DeleteAll_generic()
        {
            _db.Insert(_insertList);
            _db.DeleteAll<Base_UnitTest>();
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAllAsync_generic()
        {
            _db.Insert(_insertList);
            await _db.DeleteAllAsync<Base_UnitTest>();
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_single()
        {
            _db.Insert(_newData);
            _db.Delete(_newData);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_single()
        {
            _db.Insert(_newData);
            await _db.DeleteAsync(_newData);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_multiple()
        {
            _db.Insert(_insertList);
            _db.Delete(_insertList);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_multiple()
        {
            _db.Insert(_insertList);
            await _db.DeleteAsync(_insertList);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_where()
        {
            _db.Insert(_insertList);
            _db.Delete<Base_UnitTest>(x => x.UserId == "Admin2");
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task DeleteAsync_where()
        {
            _db.Insert(_insertList);
            await _db.DeleteAsync<Base_UnitTest>(x => x.UserId == "Admin2");
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Delete_key_generic()
        {
            _db.Insert(_newData);
            _db.Delete<Base_UnitTest>(_newData.Id);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_key_generic()
        {
            _db.Insert(_newData);
            await _db.DeleteAsync<Base_UnitTest>(_newData.Id);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_keys_generic()
        {
            _db.Insert(_insertList);
            _db.Delete<Base_UnitTest>(_insertList.Select(x => x.Id).ToList());
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_keys_generic()
        {
            _db.Insert(_insertList);
            await _db.DeleteAsync<Base_UnitTest>(_insertList.Select(x => x.Id).ToList());
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_Sql_generic()
        {
            _db.Insert(_insertList);
            _db.DeleteSql<Base_UnitTest>(x => x.UserId == "Admin2");
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task Delete_SqlAsync_generic()
        {
            _db.Insert(_insertList);
            string userId = "Admin1";

            await _db.DeleteSqlAsync<Base_UnitTest>(x => x.UserId == userId);
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);

            var deleteIds = new string[] { "Admin2" };
            await _db.DeleteSqlAsync<Base_UnitTest>(x => deleteIds.Contains(x.UserId));
            count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Update_single()
        {
            _db.Insert(_newData);
            var updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _db.Update(updateData);
            var dbUpdateData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());
        }

        [TestMethod]
        public async Task UpdateAsync_single()
        {
            _db.Insert(_newData);
            var updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            await _db.UpdateAsync(updateData);
            var dbUpdateData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());
        }

        [TestMethod]
        public void Update_multiple()
        {
            _db.Insert(_insertList);
            var updateList = _insertList.DeepClone();
            updateList[0].UserId = "Admin3";
            updateList[1].UserId = "Admin4";
            _db.Update(updateList);
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task UpdateAsync_multiple()
        {
            _db.Insert(_insertList);
            var updateList = _insertList.DeepClone();
            updateList[0].UserId = "Admin3";
            updateList[1].UserId = "Admin4";
            await _db.UpdateAsync(updateList);
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void UpdateAny_single()
        {
            _db.Insert(_newData);
            var newUpdateData = _newData.DeepClone();
            newUpdateData.UserName = "普通管理员";
            newUpdateData.UserId = "xiaoming";
            newUpdateData.Age = 100;
            _db.Update(newUpdateData, new List<string> { "UserName", "Age" });
            var dbSingleData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            newUpdateData.UserId = "Admin";
            Assert.AreEqual(newUpdateData.ToJson(), dbSingleData.ToJson());
        }

        [TestMethod]
        public async Task UpdateAnyAsync_single()
        {
            _db.Insert(_newData);
            var newUpdateData = _newData.DeepClone();
            newUpdateData.UserName = "普通管理员";
            newUpdateData.UserId = "xiaoming";
            newUpdateData.Age = 100;
            await _db.UpdateAsync(newUpdateData, new List<string> { "UserName", "Age" });
            var dbSingleData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            newUpdateData.UserId = "Admin";
            Assert.AreEqual(newUpdateData.ToJson(), dbSingleData.ToJson());
        }

        [TestMethod]
        public void UpdateAny_multiple()
        {
            _db.Insert(_insertList);
            var newList1 = _insertList.DeepClone();
            var newList2 = _insertList.DeepClone();
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

            _db.Update(newList1, new List<string> { "UserName", "Age" });
            var dbData = _db.GetIQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(newList2.OrderBy(x => x.Id).ToJson(), dbData.OrderBy(x => x.Id).ToJson());
        }

        [TestMethod]
        public async Task UpdateAnyAsync_multiple()
        {
            _db.Insert(_insertList);
            var newList1 = _insertList.DeepClone();
            var newList2 = _insertList.DeepClone();
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

            await _db.UpdateAsync(newList1, new List<string> { "UserName", "Age" });
            var dbData = _db.GetIQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(newList2.OrderBy(x => x.Id).ToJson(), dbData.OrderBy(x => x.Id).ToJson());
        }

        [TestMethod]
        public void UpdateWhere()
        {
            _db.Insert(_newData);
            _db.Update<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public async Task UpdateWhereAsync()
        {
            _db.Insert(_newData);
            await _db.UpdateAsync<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public void UpdateSql()
        {
            _db.Insert(_newData);
            var userIds = new string[] { "Admin" };
            _db.UpdateSql<Base_UnitTest>(x => userIds.Contains(x.UserId), ("UserId", UpdateType.Equal, "Admin2"));

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            _db.Insert(_newData);
            await _db.UpdateSqlAsync<Base_UnitTest>(x => x.UserId == "Admin", ("UserId", UpdateType.Equal, "Admin2"));

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public void GetEntity()
        {
            _db.Insert(_newData);
            var theData = _db.GetEntity<Base_UnitTest>(_newData.Id);
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public async Task GetEntityAsync()
        {
            _db.Insert(_newData);
            var theData = await _db.GetEntityAsync<Base_UnitTest>(_newData.Id);
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public void GetIQueryable()
        {
            _db.Insert(_newData);
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin").Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void ExcuteSql()
        {
            _db.Insert(_newData);
            string sql = "delete from Base_UnitTest";
            _db.ExecuteSql(sql);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task ExcuteSqlAsync()
        {
            _db.Insert(_newData);
            string sql = "delete from Base_UnitTest";
            await _db.ExecuteSqlAsync(sql);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void ExcuteSql_paramter()
        {
            _db.Insert(_newData);
            var sql = "delete from Base_UnitTest where UserName like '%'+@name+'%'";
            _db.ExecuteSql(sql, ("@name", "管理员"));
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task ExcuteSqlAsync_paramter()
        {
            _db.Insert(_newData);
            var sql = "delete from Base_UnitTest where UserName like '%'+@name+'%'";
            await _db.ExecuteSqlAsync(sql, ("@name", "管理员"));
            var count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void RunTransaction_fail()
        {
            bool succcess = _db.RunTransaction(() =>
            {
                _db.Insert(_newData);
                var newData2 = _newData.DeepClone();
                _db.Insert(newData2);
            }).Success;
            Assert.IsFalse(succcess);
        }

        [TestMethod]
        public async Task RunTransactionAsync_fail()
        {
            bool succcess = (await _db.RunTransactionAsync(async () =>
            {
                await _db.InsertAsync(_newData);
                var newData2 = _newData.DeepClone();
                await _db.InsertAsync(newData2);
            })).Success;
            Assert.IsFalse(succcess);
        }

        [TestMethod]
        public void RunTransaction_success()
        {
            bool succcess = _db.RunTransaction(() =>
            {
                var newData = _newData.DeepClone();
                newData.Id = Guid.NewGuid().ToString();
                newData.UserId = Guid.NewGuid().ToString();
                newData.UserName = Guid.NewGuid().ToString();
                _db.Insert(_newData);
                _db.Insert(newData);
            }).Success;
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.IsTrue(succcess);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task RunTransactionAsync_success()
        {
            bool succcess = (await _db.RunTransactionAsync(async () =>
            {
                var newData = _newData.DeepClone();
                newData.Id = Guid.NewGuid().ToString();
                newData.UserId = Guid.NewGuid().ToString();
                newData.UserName = Guid.NewGuid().ToString();
                await _db.InsertAsync(_newData);
                await _db.InsertAsync(newData);
            })).Success;
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.IsTrue(succcess);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void RunTransaction_isolationLevel()
        {
            var db1 = ServiceProvider.GetService<ISQLiteDb1>();
            var db2 = ServiceScopeFactory.CreateScope().ServiceProvider.GetService<ISQLiteDb1>();

            db1.Insert(_newData);

            var updateData = _newData.DeepClone();
            Task db2Task = new Task(() =>
            {
                updateData.UserName = Guid.NewGuid().ToString();
                db2.Update(updateData);
            });

            var res = db1.RunTransaction(() =>
            {
                //db1读=>db2写(阻塞)=>db1读=>db1提交
                var db1Data_1 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();

                db2Task.Start();

                var db1Data_2 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
                Assert.AreEqual(db1Data_1.ToJson(), db1Data_2.ToJson());
            });
            db2Task.Wait();
            var db1Data_3 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), db1Data_3.ToJson());
        }

        [TestMethod]
        public void DistributedTransaction()
        {
            //失败事务
            var db1 = ServiceProvider.GetService<ISQLiteDb1>();
            var db2 = ServiceProvider.GetService<ISQLiteDb2>();
            db1.DeleteAll<Base_UnitTest>();
            db2.DeleteAll<Base_UnitTest>();
            Base_UnitTest data1 = new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "1",
                UserName = Guid.NewGuid().ToString()
            };
            Base_UnitTest data2 = new Base_UnitTest
            {
                Id = data1.Id,
                UserId = "1",
                UserName = Guid.NewGuid().ToString()
            };
            Base_UnitTest data3 = new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "2",
                UserName = Guid.NewGuid().ToString()
            };

            new Action(() =>
            {
                var transaction = DistributedTransactionFactory.GetDistributedTransaction();
                transaction.AddDbAccessor(db1, db2);
                var succcess = transaction.RunTransaction(() =>
                    {
                        db1.ExecuteSql("insert into Base_UnitTest(Id,CreateTime) values('10',@CreateTime) ", ("@CreateTime", DateTime.Now));
                        db1.Insert(data1);
                        db1.Insert(data2);
                        db2.Insert(data1);
                        db2.Insert(data3);
                    });
                Assert.IsFalse(succcess.Success);
                Assert.AreEqual(0, db1.GetIQueryable<Base_UnitTest>().Count());
                Assert.AreEqual(0, db2.GetIQueryable<Base_UnitTest>().Count());
            })();

            //成功事务
            new Action(() =>
            {
                var transaction = DistributedTransactionFactory.GetDistributedTransaction();
                transaction.AddDbAccessor(db1, db2);

                var succcess = transaction
                    .RunTransaction(() =>
                    {
                        db1.ExecuteSql("insert into Base_UnitTest(Id,CreateTime) values('10',@CreateTime) ", ("@CreateTime", DateTime.Now));
                        db1.Insert(data1);
                        db1.Insert(data3);
                        db2.Insert(data1);
                        db2.Insert(data3);
                    });
                int count1 = db1.GetIQueryable<Base_UnitTest>().Count();
                int count2 = db2.GetIQueryable<Base_UnitTest>().Count();
                Assert.IsTrue(succcess.Success);
                Assert.AreEqual(3, count1);
                Assert.AreEqual(2, count2);
            })();
        }

        [TestMethod]
        public void Tracking()
        {
            using var scop = RootServiceProvider.CreateScope();
            var db = scop.ServiceProvider.GetService<IDbAccessor>();
            db.Insert(_insertList);

            var data = db.GetIQueryable<Base_UnitTest>(true).FirstOrDefault();
            data.Age = 10000;
            db.SaveChanges();

            var newData = db.GetIQueryable<Base_UnitTest>(true).FirstOrDefault();
            Assert.AreEqual(data.ToJson(), newData.ToJson());
        }

        [TestMethod]
        public void Dispose()
        {
            using (var scop = RootServiceProvider.CreateScope())
            {
                scop.ServiceProvider.GetService<IDbAccessor>().Dispose();
                scop.ServiceProvider.GetService<ICustomDbAccessor>().Dispose();
            }
        }
    }
}
