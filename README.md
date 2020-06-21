- [简介](#简介)
- [引言](#引言)
- [开始](#开始)
  - [准备](#准备)
  - [配置](#配置)
  - [使用](#使用)
  - [按时间自动分表](#按时间自动分表)
  - [性能测试](#性能测试)
  - [其它简单操作(非Sharing)](#其它简单操作非sharing)
- [常用配置](#常用配置)
  - [多主键支持](#多主键支持)
  - [索引支持](#索引支持)
- [注意事项](#注意事项)
- [总结](#总结)

# 简介
本框架旨在为EF Core提供**Sharding**(即读写分离分库分表)支持,不仅提供了一套强大的普通数据操作接口,并且降低了分表难度,支持按时间自动分表扩容,提供的操作接口简洁统一.

源码地址:[EFCore.SHarding](https://github.com/Coldairarrow/EFCore.Sharding)

# 引言
读写分离分库分表一直是数据库领域中的重难点,当数据规模达到单库极限的时候,就不得不考虑分表方案。EF Core作为.NET Core中最为主流的ORM,用起来十分方便快捷,但是官方并没有相应的Sharding支持,鄙人不才,经过一番摸索之后终于完成这个框架.

# 开始
## 准备
首先根据需要安装对应的Nuget包 

| 包名 | 说明 |
|--|--|
| EFCore.Sharding | 必装包,3.x版本对应EF Core3.x,2.x版本对应EF Core2.x |
| EFCore.Sharding.MySql | MySql支持 |
| EFCore.Sharding.PostgreSql | PostgreSql支持 |
| EFCore.Sharding.SQLite | SQLite支持 |
| EFCore.Sharding.SqlServer | SqlServer支持(3.x版本需要SqlServer2012+,若要用低版本则用2.x版本) |
| EFCore.Sharding.Oracle | Oracle支持(暂不支持3.x) |

## 配置
```c#

ShardingConfig.Init(config =>
{
    config.AddAbsDb(DatabaseType.SQLite)
        .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, "DataSource=db.db")
        .AddPhysicDbGroup()
        .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
});
```
上述代码中完成了Sharding配置

- **AddAbsDb**是指添加抽象数据库,抽象数据库就是将多个分库看成同一个数据库来进行操作
- **AddPhysicDbGroup**是指添加物理数据库组,在同一组物理数据库中,它们数据库类型相同,拥有的表相同,每个数据库拥有的数据是一致的(之间通过主主复制或主从复制进行数据同步)
- **AddPhysicTable**是指添加物理数据表,传入的Base_UnitTest是抽象数据表(即将Base_UnitTest拆分为Base_UnitTest_0~2)
- **SetHashModShardingRule**是采用哈希取模的分表规则,分表字段为**Id**,取模值为**3**,会自动生成表Base_UnitTest_0,Base_UnitTest_1,Base_UnitTest_2

## 使用
配置完成，下面开始使用，使用方式**非常简单**，与平常使用基本一致
首先获取分片仓储接口IShardingDbAccessor

```c#
using(IShardingDbAccessor _db = DbFactory.GetShardingDbAccessor())
{

}
```
然后即可进行数据操作：

```c#
Base_UnitTest _newData  = new Base_UnitTest
{
    Id = Guid.NewGuid().ToString(),
    UserId = "Admin",
    UserName = "超级管理员",
    Age = 22
};
List<Base_UnitTest> _insertList = new List<Base_UnitTest>
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
//添加单条数据
_db.Insert(_newData);
//添加多条数据
_db.Insert(_insertList);
//清空表
_db.DeleteAll<Base_UnitTest>();
//删除单条数据
_db.Delete(_newData);
//删除多条数据
_db.Delete(_insertList);
//删除指定数据
_db.Delete<Base_UnitTest>(x => x.UserId == "Admin2");
//更新单条数据
_db.Update(_newData);
//更新多条数据
_db.Update(_insertList);
//更新单条数据指定属性
_db.UpdateAny(_newData, new List<string> { "UserName", "Age" });
//更新多条数据指定属性
_db.UpdateAny(_insertList, new List<string> { "UserName", "Age" });
//更新指定条件数据
_db.UpdateWhere<Base_UnitTest>(x => x.UserId == "Admin", x =>
{
    x.UserId = "Admin2";
});
//GetList获取表的所有数据
var list=_db.GetList<Base_UnitTest>();
//GetIQPagination获取分页后的数据
var list=_db.GetIShardingQueryable<Base_UnitTest>().GetPagination(pagination);
//Max
var max=_db.GetIShardingQueryable<Base_UnitTest>().Max(x => x.Age);
//Min
var min=_db.GetIShardingQueryable<Base_UnitTest>().Min(x => x.Age);
//Average
var min=_db.GetIShardingQueryable<Base_UnitTest>().Average(x => x.Age);
//Count
var min=_db.GetIShardingQueryable<Base_UnitTest>().Count();
//事务,使用方式与普通事务一致
bool succcess = _db.RunTransaction(() =>
{
    _db.Insert(_newData);
    var newData2 = _newData.DeepClone();
    _db.Insert(newData2);
}).Success;
Assert.AreEqual(succcess, false);
```
上述操作中表面上是操作Base_UnitTest表，实际上却在按照一定规则使用Base_UnitTest_0~2三张表，使分片对业务**操作透明**，极大提高开发效率  
具体使用方式请参考单元测试源码：[连接](https://github.com/Coldairarrow/EFCore.Sharding/tree/master/src/EFCore.Sharding.Tests/Sharding)

## 按时间自动分表
上面的哈希取模的方式虽然简单,但是却十分不实用,因为当3张分表到达瓶颈时,将会面临扩容的问题，这种方式扩容需要进行大量的数据迁移，这无疑是十分麻烦的。因此需要一种方式能够系统自动建表扩容，并且无需人工干预，这就是按时间自动分表.

```c#

using EFCore.Sharding;
using EFCore.Sharding.Tests;
using System;
using System.Threading;

namespace Demo.AutoExpandByDate
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now.AddMinutes(-5);
            DateTime endTime = DateTime.MaxValue;

            //配置初始化
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)//添加抽象数据库
                    .AddPhysicDbGroup()//添加物理数据库组
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.CONSTRING1)//添加物理数据库1
                    .SetDateShardingRule<Base_UnitTest>(nameof(Base_UnitTest.CreateTime))//设置分表规则
                    .AutoExpandByDate<Base_UnitTest>(//设置为按时间自动分表
                        ExpandByDateMode.PerMinute,
                        (startTime, endTime, ShardingConfig.DefaultDbGourpName)
                        );
            });
            var db = DbFactory.GetShardingDbAccessor();
            while (true)
            {
                try
                {
                    db.Insert(new Base_UnitTest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Age = 1,
                        UserName = Guid.NewGuid().ToString(),
                        CreateTime = DateTime.Now
                    });

                    DateTime time = DateTime.Now.AddMinutes(-2);
                    var count = db.GetIShardingQueryable<Base_UnitTest>()
                        .Where(x => x.CreateTime >= time)
                        .Count();
                    Console.WriteLine($"当前数据量:{count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(50);
            }
        }
    }
}

```
上面Demo都在源码中  

上面的代码实现了将Base_UnitTest表按照时间自动分表，每分钟创建一张表，实际使用中根据业务需求设置ExpandByDateMode参数，常用按天、按月分表

自动分表效果
![JKxE8K.png](https://s1.ax1x.com/2020/04/19/JKxE8K.png)
全程无需人工干预，系统会自动定时创建分表，十分简单好用

## 性能测试
```c#
using EFCore.Sharding;
using EFCore.Sharding.Tests;
using System;
using System.Diagnostics;
using System.Linq;

namespace Demo.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            ShardingConfig.Init(config =>
            {
                config.AddAbsDb(DatabaseType.SqlServer)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, Config.CONSTRING1)
                    .AddPhysicDbGroup()
                    .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3);
            });

            DateTime time1 = DateTime.Now;
            DateTime time2 = DateTime.Now;

            var db = DbFactory.GetDbAccessor(Config.CONSTRING1, DatabaseType.SqlServer);
            Stopwatch watch = new Stopwatch();

            var q = db.GetIQueryable<Base_UnitTest>()
                .Where(x => x.UserName.Contains("00001C22-8DD2-4D47-B500-407554B099AB"))
                .OrderByDescending(x => x.Id)
                .Skip(0)
                .Take(30);

            q.ToList();
            q.ToSharding().ToList();
            watch.Restart();
            var list1 = q.ToList();
            watch.Stop();
            Console.WriteLine($"未分表耗时:{watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var list2 = q.ToSharding().ToList();
            watch.Stop();
            Console.WriteLine($"分表后耗时:{watch.ElapsedMilliseconds}ms");

            Console.WriteLine("完成");

            Console.ReadLine();
        }
    }
}

```

分表Base_UnitTest_0-2各有100万数据,然后将这三张表的数据导入Base_UnitTest中(即Base_UnitTest表的数据与Base_UnitTest_0-2三张表总合数据一致) 

分表与不分表测试结果如下

![JMSJBQ.png](https://s1.ax1x.com/2020/04/19/JMSJBQ.png)

这里仅仅分了3张表，其效果立杆见影，若分表几十张，那效果想想就很棒

## 其它简单操作(非Sharing)
框架不仅支持Sharing,而且封装了常用数据库操作,使用比较简单  
详细使用方式参考 [链接](https://github.com/Coldairarrow/EFCore.Sharding/blob/master/examples/Demo.DI/Program.cs)

# 常用配置
## 多主键支持
在实体类上加上EFCore.Sharding.DataAnnotations.Keys特性即可  
自动建表时会自动创建主键  
```c#
    /// <summary>
    /// 单元测试表
    /// </summary>
    [Table("Base_UnitTest")]
    [Index(false, nameof(CreateTime))]
    [Index(false, nameof(Age))]
    [Keys(nameof(Id), nameof(UserName))]
    public class Base_UnitTest
    {
        /// <summary>
        /// 代理主键
        /// </summary>
        [Key, StringLength(50)]
        public String Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public Int32? Age { get; set; }
    }

```

## 索引支持
在实体类上加上EFCore.Sharding.DataAnnotations.Index特性即可  
可以设置多个Index特性  
自动建表时会自动创建索引

```c#
    /// <summary>
    /// 单元测试表
    /// </summary>
    [Table("Base_UnitTest")]
    [Index(false, nameof(CreateTime))]
    [Index(false, nameof(Age))]
    [Keys(nameof(Id), nameof(UserName))]
    public class Base_UnitTest
    {
        /// <summary>
        /// 代理主键
        /// </summary>
        [Key, StringLength(50)]
        public String Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public Int32? Age { get; set; }
    }

```

# 注意事项

- 查询尽量使用分表字段进行筛选，避免全表扫描

# 总结
这个简单实用强大的框架希望能够帮助到大家,力求为.NET生态贡献一份力,大家一起壮大.NET生态

欢迎使用本框架，若觉得不错，请比心

![](https://raw.githubusercontent.com/Coldairarrow/UploadFiles/master/Colder.Fx.Net.AdminLTE/04abaa3d37fa01b4c4058c8163aab6a8.jpg)

Github欢迎星星:<https://github.com/Coldairarrow>

博客园欢迎点赞：<https://www.cnblogs.com/coldairarrow/>

QQ群3:940069478  
个人QQ:862520575（**欢迎技术支持及商务合作**）    

<center> 本人将会对这个快速开发框架不断完善与维护，希望能够帮助到各位<center/>
<center> 若遇到任何问题或需要技术支持，请联系我<center/>
<center> ------学习永无止境，技术永无上限，代码就是艺术------<center/>
