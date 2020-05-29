using EFCore.Sharding.DataAnnotations;
using EFCore.Sharding.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;

namespace EFCore.Sharding
{
    internal static class DbModelFactory
    {
        #region 构造函数

        static DbModelFactory()
        {
            InitEntityType();
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取DbCompiledModel
        /// </summary>
        /// <param name="conStr">数据库连接名或字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static IModel GetDbCompiledModel(string conStr, DatabaseType dbType)
        {
            string modelInfoId = GetCompiledModelIdentity(conStr, dbType);
            bool success = _dbCompiledModel.TryGetValue(modelInfoId, out IModel resModel);
            if (!success)
            {
                var theLock = _lockDic.GetOrAdd(modelInfoId, new object());
                lock (theLock)
                {
                    success = _dbCompiledModel.TryGetValue(modelInfoId, out resModel);
                    if (!success)
                    {
                        resModel = BuildDbCompiledModel(dbType);
                        _dbCompiledModel[modelInfoId] = resModel;
                    }
                }
            }

            return resModel;
        }

        /// <summary>
        /// 获取实体模型
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static Type GetEntityType(string tableName)
        {
            if (!_entityTypeMap.ContainsKey(tableName))
                throw new Exception($"表[{tableName}]缺少实体模型!");

            return _entityTypeMap[tableName];
        }

        /// <summary>
        /// 添加实体模型
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="entityType">实体模型</param>
        public static void AddEntityType(string tableName, Type entityType)
        {
            if (_entityTypeMap.ContainsKey(tableName))
                return;

            _entityTypeMap[tableName] = entityType;
            _dbCompiledModel.Clear();
        }

        public static IModel BuildDbCompiledModel(DatabaseType dbType, List<Type> entityTypes = null)
        {
            ModelBuilder modelBuilder = DbFactory.GetProvider(dbType).GetModelBuilder();
            List<Type> needTypes = entityTypes?.Count > 0 ? entityTypes : _entityTypeMap.Values.ToList();
            needTypes.ForEach(x =>
            {
                var entityTypeBuilder = modelBuilder.Entity(x);

                //主键
                var keys = x.GetCustomAttribute<KeysAttribute>();
                if (keys != null)
                    entityTypeBuilder.HasKey(keys.PropertyNames);

                //索引
                var indexs = x.GetCustomAttributes<IndexAttribute>();
                if (indexs != null)
                {
                    indexs.ToList().ForEach(aIndex =>
                    {
                        entityTypeBuilder.HasIndex(aIndex.PropertyNames).IsUnique(aIndex.IsUnique);
                    });
                }
                // 如果是逻辑删除，那么可以在这里直接做查询过滤
                // 这样的话。如果是在Bussiness层做Include的时候，也不会把Deleted的数据加载出来
                // 如果不是做HasQueryFilter的话。Include不会过滤Deleted的数据
                if (ShardingConfig.LogicDelete)
                {
                    if (entityTypeBuilder.Metadata.FindProperty(ShardingConfig.DeletedField) == null)
                    {
                        var exp = DynamicExpressionParser.ParseLambda(x, typeof(bool), $"{ShardingConfig.DeletedField} == @0", false);
                        entityTypeBuilder.HasQueryFilter(exp);
                    }
                }
            });
            //支持IEntityTypeConfiguration配置
            needTypes.Select(x => x.Assembly).ToList().ForEach(aAssembly =>
            {
                modelBuilder.ApplyConfigurationsFromAssembly(aAssembly);
            });

            return modelBuilder.FinalizeModel();
        }

        #endregion

        #region 私有成员

        private static void InitEntityType()
        {
            List<Type> types = ShardingConfig.AllEntityTypes
                .Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null)
                .ToList();

            types.ForEach(aType =>
            {
                _entityTypeMap[aType.Name] = aType;
            });
        }
        private static ConcurrentDictionary<string, Type> _entityTypeMap { get; } =
            new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, IModel> _dbCompiledModel { get; }
            = new ConcurrentDictionary<string, IModel>();
        private static string GetCompiledModelIdentity(string conStr, DatabaseType dbType)
        {
            return $"{dbType.ToString()}{conStr}";
        }
        private static readonly ConcurrentDictionary<string, object> _lockDic
            = new ConcurrentDictionary<string, object>();

        #endregion

        #region 数据结构

        #endregion
    }
}
