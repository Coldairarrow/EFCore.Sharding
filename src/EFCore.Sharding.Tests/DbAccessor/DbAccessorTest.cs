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
            _ = _db.DeleteAll<Base_UnitTest>();
        }

        [TestMethod]
        public void Insert_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest theData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public async Task InsertAsync_single()
        {
            _ = await _db.InsertAsync(_newData);
            Base_UnitTest theData = await _db.GetIQueryable<Base_UnitTest>().FirstOrDefaultAsync();
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public void Insert_multiple()
        {
            _ = _db.Insert(_insertList);
            List<Base_UnitTest> theList = _db.GetIQueryable<Base_UnitTest>().ToList();
            Assert.AreEqual(_insertList.OrderBy(X => X.Id).ToJson(), theList.OrderBy(X => X.Id).ToJson());
        }

        [TestMethod]
        public async Task InsertAsync_multiple()
        {
            _ = await _db.InsertAsync(_insertList);
            List<Base_UnitTest> theList = await _db.GetIQueryable<Base_UnitTest>().ToListAsync();
            Assert.AreEqual(_insertList.OrderBy(X => X.Id).ToJson(), theList.OrderBy(X => X.Id).ToJson());
        }

        [TestMethod]
        public void DeleteAll_generic()
        {
            _ = _db.Insert(_insertList);
            _ = _db.DeleteAll<Base_UnitTest>();
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAllAsync_generic()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAllAsync<Base_UnitTest>();
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_single()
        {
            _ = _db.Insert(_newData);
            _ = _db.Delete(_newData);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_single()
        {
            _ = _db.Insert(_newData);
            _ = await _db.DeleteAsync(_newData);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_multiple()
        {
            _ = _db.Insert(_insertList);
            _ = _db.Delete(_insertList);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_multiple()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAsync(_insertList);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_where()
        {
            _ = _db.Insert(_insertList);
            _ = _db.Delete<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task DeleteAsync_where()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAsync<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void Delete_key_generic()
        {
            _ = _db.Insert(_newData);
            _ = _db.Delete<Base_UnitTest>(_newData.Id);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_key_generic()
        {
            _ = _db.Insert(_newData);
            _ = await _db.DeleteAsync<Base_UnitTest>(_newData.Id);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_keys_generic()
        {
            _ = _db.Insert(_insertList);
            _ = _db.Delete<Base_UnitTest>(_insertList.Select(x => x.Id).ToList());
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task DeleteAsync_keys_generic()
        {
            _ = _db.Insert(_insertList);
            _ = await _db.DeleteAsync<Base_UnitTest>(_insertList.Select(x => x.Id).ToList());
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Delete_Sql_generic()
        {
            _ = _db.Insert(_insertList);
            _ = _db.DeleteSql<Base_UnitTest>(x => x.UserId == "Admin2");
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task Delete_SqlAsync_generic()
        {
            _ = _db.Insert(_insertList);
            string userId = "Admin1";

            _ = await _db.DeleteSqlAsync<Base_UnitTest>(x => x.UserId == userId);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(1, count);

            string[] deleteIds = new string[] { "Admin2" };
            _ = await _db.DeleteSqlAsync<Base_UnitTest>(x => deleteIds.Contains(x.UserId));
            count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Update_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _ = _db.Update(updateData);
            Base_UnitTest dbUpdateData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), dbUpdateData.ToJson());
        }

        [TestMethod]
        public async Task UpdateAsync_single()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest updateData = _newData.DeepClone();
            updateData.UserId = "Admin_Update";
            _ = await _db.UpdateAsync(updateData);
            Base_UnitTest dbUpdateData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
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
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
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
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin3" || x.UserId == "Admin4").Count();
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
            Base_UnitTest dbSingleData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
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
            Base_UnitTest dbSingleData = _db.GetIQueryable<Base_UnitTest>().FirstOrDefault();
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
            List<Base_UnitTest> dbData = _db.GetIQueryable<Base_UnitTest>().ToList();
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
            List<Base_UnitTest> dbData = _db.GetIQueryable<Base_UnitTest>().ToList();
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

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public async Task UpdateWhereAsync()
        {
            _ = _db.Insert(_newData);
            _ = await _db.UpdateAsync<Base_UnitTest>(x => x.UserId == "Admin", x =>
            {
                x.UserId = "Admin2";
            });

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public void UpdateSql()
        {
            _ = _db.Insert(_newData);
            string[] userIds = new string[] { "Admin" };
            _ = _db.UpdateSql<Base_UnitTest>(x => userIds.Contains(x.UserId), ("UserId", UpdateType.Equal, "Admin2"));

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            _ = _db.Insert(_newData);
            _ = await _db.UpdateSqlAsync<Base_UnitTest>(x => x.UserId == "Admin", ("UserId", UpdateType.Equal, "Admin2"));

            Assert.IsTrue(_db.GetIQueryable<Base_UnitTest>().Any(x => x.UserId == "Admin2"));
        }

        [TestMethod]
        public void GetEntity()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest theData = _db.GetEntity<Base_UnitTest>(_newData.Id);
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public async Task GetEntityAsync()
        {
            _ = _db.Insert(_newData);
            Base_UnitTest theData = await _db.GetEntityAsync<Base_UnitTest>(_newData.Id);
            Assert.AreEqual(_newData.ToJson(), theData.ToJson());
        }

        [TestMethod]
        public void GetIQueryable()
        {
            _ = _db.Insert(_newData);
            int count = _db.GetIQueryable<Base_UnitTest>().Where(x => x.UserId == "Admin").Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void ExcuteSql()
        {
            _ = _db.Insert(_newData);
            string sql = "delete from Base_UnitTest";
            _ = _db.ExecuteSql(sql);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task ExcuteSqlAsync()
        {
            _ = _db.Insert(_newData);
            string sql = "delete from Base_UnitTest";
            _ = await _db.ExecuteSqlAsync(sql);
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void ExcuteSql_paramter()
        {
            _ = _db.Insert(_newData);
            string sql = "delete from Base_UnitTest where UserName like '%'+@name+'%'";
            _ = _db.ExecuteSql(sql, ("@name", "管理员"));
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task ExcuteSqlAsync_paramter()
        {
            _ = _db.Insert(_newData);
            string sql = "delete from Base_UnitTest where UserName like '%'+@name+'%'";
            _ = await _db.ExecuteSqlAsync(sql, ("@name", "管理员"));
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.AreEqual(0, count);
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
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
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
            int count = _db.GetIQueryable<Base_UnitTest>().Count();
            Assert.IsTrue(succcess);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void RunTransaction_isolationLevel()
        {
            ISQLiteDb1 db1 = ServiceProvider.GetService<ISQLiteDb1>();
            ISQLiteDb1 db2 = ServiceScopeFactory.CreateScope().ServiceProvider.GetService<ISQLiteDb1>();

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
                Base_UnitTest db1Data_1 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();

                db2Task.Start();

                Base_UnitTest db1Data_2 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
                Assert.AreEqual(db1Data_1.ToJson(), db1Data_2.ToJson());
            });
            db2Task.Wait();
            Base_UnitTest db1Data_3 = db1.GetIQueryable<Base_UnitTest>().Where(x => x.Id == _newData.Id).FirstOrDefault();
            Assert.AreEqual(updateData.ToJson(), db1Data_3.ToJson());
        }

        [TestMethod]
        public void DistributedTransaction()
        {
            //失败事务
            ISQLiteDb1 db1 = ServiceProvider.GetService<ISQLiteDb1>();
            ISQLiteDb2 db2 = ServiceProvider.GetService<ISQLiteDb2>();
            _ = db1.DeleteAll<Base_UnitTest>();
            _ = db2.DeleteAll<Base_UnitTest>();
            Base_UnitTest data1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "1",
                UserName = Guid.NewGuid().ToString()
            };
            Base_UnitTest data2 = new()
            {
                Id = data1.Id,
                UserId = "1",
                UserName = Guid.NewGuid().ToString()
            };
            Base_UnitTest data3 = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "2",
                UserName = Guid.NewGuid().ToString()
            };

            new Action(() =>
            {
                IDistributedTransaction transaction = DistributedTransactionFactory.GetDistributedTransaction();
                transaction.AddDbAccessor(db1, db2);
                (bool Success, Exception ex) = transaction.RunTransaction(() =>
                    {
                        _ = db1.ExecuteSql("insert into Base_UnitTest(Id,CreateTime) values('10',@CreateTime) ", ("@CreateTime", DateTime.Now));
                        _ = db1.Insert(data1);
                        _ = db1.Insert(data2);
                        _ = db2.Insert(data1);
                        _ = db2.Insert(data3);
                    });
                Assert.IsFalse(Success);
                Assert.AreEqual(0, db1.GetIQueryable<Base_UnitTest>().Count());
                Assert.AreEqual(0, db2.GetIQueryable<Base_UnitTest>().Count());
            })();

            //成功事务
            new Action(() =>
            {
                IDistributedTransaction transaction = DistributedTransactionFactory.GetDistributedTransaction();
                transaction.AddDbAccessor(db1, db2);

                (bool Success, Exception ex) = transaction
                    .RunTransaction(() =>
                    {
                        _ = db1.ExecuteSql("insert into Base_UnitTest(Id,CreateTime) values('10',@CreateTime) ", ("@CreateTime", DateTime.Now));
                        _ = db1.Insert(data1);
                        _ = db1.Insert(data3);
                        _ = db2.Insert(data1);
                        _ = db2.Insert(data3);
                    });
                int count1 = db1.GetIQueryable<Base_UnitTest>().Count();
                int count2 = db2.GetIQueryable<Base_UnitTest>().Count();
                Assert.IsTrue(Success);
                Assert.AreEqual(3, count1);
                Assert.AreEqual(2, count2);
            })();
        }

        [TestMethod]
        public void Tracking()
        {
            using IServiceScope scop = RootServiceProvider.CreateScope();
            IDbAccessor db = scop.ServiceProvider.GetService<IDbAccessor>();
            _ = db.Insert(_insertList);

            Base_UnitTest data = db.GetIQueryable<Base_UnitTest>(true).FirstOrDefault();
            data.Age = 10000;
            _ = db.SaveChanges();

            Base_UnitTest newData = db.GetIQueryable<Base_UnitTest>(true).FirstOrDefault();
            Assert.AreEqual(data.ToJson(), newData.ToJson());
        }

        [TestMethod]
        public void Dispose()
        {
            using IServiceScope scop = RootServiceProvider.CreateScope();
            scop.ServiceProvider.GetService<IDbAccessor>().Dispose();
            scop.ServiceProvider.GetService<ICustomDbAccessor>().Dispose();
        }
    }
}
