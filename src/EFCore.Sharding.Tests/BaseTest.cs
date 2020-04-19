using Coldairarrow.Util;
using System;
using System.Collections.Generic;

namespace EFCore.Sharding.Tests
{
    class Base_UnitTestShardingRule : ModShardingRule<Base_UnitTest>
    {
        protected override string KeyField => "Id";
        protected override int Mod => 3;
    }

    public abstract class BaseTest
    {
        static BaseTest()
        {
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
        public BaseTest()
        {
            Clear();
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
