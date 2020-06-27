﻿using System;

namespace EFCore.Sharding
{
    /// <summary>
    /// 配置初始化
    /// </summary>
    public interface IConfigInit
    {
        /// <summary>
        /// 设置实体的程序集路径
        /// </summary>
        /// <param name="entityAssemblyPaths">程序集路径</param>
        /// <returns></returns>
        IConfigInit SetEntityAssemblyPath(params string[] entityAssemblyPaths);

        /// <summary>
        /// 设置实体的程序集
        /// 注:使用模糊匹配获取,例如传入"Entity",则将会扫描所有名字包含"Entity"的程序集
        /// 注:实体类必须拥有System.ComponentModel.DataAnnotations.Schema.TableAttribute特性
        /// </summary>
        /// <param name="entityAssemblyNames">程序集名</param>
        /// <returns></returns>
        IConfigInit SetEntityAssembly(params string[] entityAssemblyNames);

        /// <summary>
        /// 使用逻辑删除
        /// </summary>
        /// <param name="keyField">主键字段,字段类型为string</param>
        /// <param name="deletedField">已删除标志字段,字段类型为bool</param>
        /// <returns></returns>
        IConfigInit UseLogicDelete(string keyField = "Id", string deletedField = "Deleted");

        /// <summary>
        /// 使用数据库
        /// </summary>
        /// <typeparam name="TDbAccessor">自定义的IDbAccessor</typeparam>
        /// <param name="conString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        IConfigInit UseDatabase<TDbAccessor>(string conString, DatabaseType dbType) where TDbAccessor : class, IDbAccessor;

        /// <summary>
        /// 使用默认数据库
        /// 注入IDbAccessor
        /// </summary>
        /// <param name="conString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        IConfigInit UseDatabase(string conString, DatabaseType dbType);

        /// <summary>
        /// 添加抽象数据库
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        IConfigInit AddAbsDb(
            DatabaseType dbType,
            string absDbName = ShardingConfig.DefaultAbsDbName);

        /// <summary>
        /// 添加物理数据库组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        IConfigInit AddPhysicDbGroup(
            string groupName = ShardingConfig.DefaultDbGourpName,
            string absDbName = ShardingConfig.DefaultAbsDbName);

        /// <summary>
        /// 添加物理数据库
        /// </summary>
        /// <param name="opType">读写类型,同时读写填写ReadWriteType.Read | ReadWriteType.Write</param>
        /// <param name="conString">连接字符串</param>
        /// <param name="groupName">数据库组名</param>
        /// <returns></returns>
        IConfigInit AddPhysicDb(
            ReadWriteType opType,
            string conString,
            string groupName = ShardingConfig.DefaultDbGourpName);

        /// <summary>
        /// 添加物理数据表
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="physicTableName">物理表明</param>
        /// <param name="groupName">数据库组名</param>
        /// <returns></returns>
        IConfigInit AddPhysicTable<TEntity>(
            string physicTableName,
            string groupName = ShardingConfig.DefaultDbGourpName);

        /// <summary>
        /// 设置分表规则(哈希取模)
        /// 注:默认自动创建分表(若分表不存在)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="mod">取模</param>
        /// <param name="createTable">是否自动建表,默认true</param>
        /// <param name="groupName">数据库组名</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        IConfigInit SetHashModShardingRule<TEntity>(
            string shardingField,
            int mod,
            bool createTable = true,
            string groupName = "BaseDbGroup",
            string absDbName = ShardingConfig.DefaultAbsDbName);

        /// <summary>
        /// 设置分表规则(按日期)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="absDbName">抽象数据库名</param>
        /// <returns></returns>
        IConfigInit SetDateShardingRule<TEntity>(
            string shardingField,
            string absDbName = ShardingConfig.DefaultAbsDbName);

        /// <summary>
        /// 通过日期自动扩容
        /// 优点:即自动定时按照建表,无需数据迁移
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="expandByDateMode">扩容模式</param>
        /// <param name="ranges">表在不同时间段的分布设置,即某个时间段的表都分布在特定的数据库组中</param>
        /// <returns></returns>
        IConfigInit AutoExpandByDate<TEntity>(
            ExpandByDateMode expandByDateMode,
            params (DateTime startTime, DateTime endTime, string groupName)[] ranges
            );
    }
}
