using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EFCore.Sharding.Tests
{
    public abstract class BaseTest : Startup
    {
        static BaseTest()
        {
            for (int i = 1; i <= 100; i++)
            {
                Base_UnitTest newData = new Base_UnitTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Age = i,
                    UserId = "Admin" + i,
                    UserName = "超级管理员" + i
                };
                _dataList.Add(newData);
            }
        }
        protected BaseTest()
        {
            ServiceScope = RootServiceProvider.CreateScope();
            ServiceProvider = ServiceScope.ServiceProvider;
        }
        protected IServiceScope ServiceScope { get; }
        protected IServiceProvider ServiceProvider { get; }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            Clear();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            ServiceScope.Dispose();
        }

        protected static List<Base_UnitTest> _insertList { get; }
            = new List<Base_UnitTest>
        {
            new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "Admin1",
                UserName = "超级管理员1",
                Age = 22
            },
            new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "Admin2",
                UserName = "超级管理员2",
                Age = 22
            }
        };
        protected static List<Base_UnitTest> _dataList { get; }
            = new List<Base_UnitTest>();
        protected static Base_UnitTest _newData { get; } = new Base_UnitTest
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "Admin",
            UserName = "超级管理员",
            Age = 22
        };
        protected abstract void Clear();
    }
}
