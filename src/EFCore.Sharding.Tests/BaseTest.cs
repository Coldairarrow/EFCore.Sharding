using Coldairarrow.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EFCore.Sharding.Tests
{
    public abstract class BaseTest
    {
        static BaseTest()
        {
            InitId();
            for (int i = 1; i <= 100; i++)
            {
                Base_UnitTest newData = new Base_UnitTest
                {
                    Id = IdHelper.GetId(),
                    Age = i,
                    UserId = "Admin" + i,
                    UserName = "超级管理员" + i
                };
                _dataList.Add(newData);
            }
        }
        private static void InitId()
        {
            new IdHelperBootstrapper()
                //设置WorkerId
                .SetWorkderId(1)
                //使用Zookeeper
                //.UseZookeeper("127.0.0.1:2181", 200, GlobalSwitch.ProjectName)
                .Boot();
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

        [TestInitialize]
        public void TestInitialize()
        {
            Clear();
        }

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
