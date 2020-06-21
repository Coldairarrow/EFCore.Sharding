using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public static class DbFactory
    {
        #region 外部接口

        internal static AbstractProvider GetProvider(DatabaseType databaseType)
        {
            string assemblyName = $"EFCore.Sharding.{databaseType}";
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);

                var type = assembly.GetType($"{assemblyName}.{databaseType}Provider");

                return Activator.CreateInstance(type) as AbstractProvider;
            }
            catch
            {
                throw new Exception($"请安装nuget包:{assemblyName}");
            }
        }

        /// <summary>
        /// 根据配置文件获取数据库类型，并返回对应的工厂接口
        /// </summary>
        /// <param name="conString">完整数据库链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns></returns>
        public static IDbAccessor GetDbAccessor(string conString, DatabaseType dbType, ILoggerFactory loggerFactory = null)
        {
            var dbContext = GetDbContext(conString, dbType, null, loggerFactory);

            return GetProvider(dbType).GetDbAccessor(dbContext);
        }

        /// <summary>
        /// 获取ShardingDbAccessor
        /// </summary>
        /// <param name="absDbName">抽象数据库</param>
        /// <returns>ShardingDbAccessor</returns>
        public static IShardingDbAccessor GetShardingDbAccessor(string absDbName = ShardingConfig.DefaultAbsDbName)
        {
            ShardingConfig.CheckInit();

            var dbType = ShardingConfig.ConfigProvider.GetAbsDbType(absDbName);

            return new ShardingDbAccessor(GetDbAccessor(string.Empty, dbType), absDbName);
        }

        internal static void CreateTable(string conString, DatabaseType dbType, Type tableEntityType)
        {
            DbContext dbContext = GetDbContext(conString, dbType, new List<Type> { tableEntityType });
            var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            try
            {
                databaseCreator.CreateTables();
            }
            catch
            {

            }
        }

        internal static BaseDbContext GetDbContext(string conString, DatabaseType dbType, List<Type> entityTypes = null, ILoggerFactory loggerFactory = null)
        {
            AbstractProvider provider = GetProvider(dbType);

            DbConnection dbConnection = provider.GetDbConnection();
            dbConnection.ConnectionString = conString;

            IModel model;
            if (entityTypes?.Count > 0)
                model = DbModelFactory.BuildDbCompiledModel(dbType, entityTypes);
            else
                model = DbModelFactory.GetDbCompiledModel(conString, dbType);
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();

            provider.UseDatabase(builder, dbConnection);

            builder.EnableSensitiveDataLogging();
            builder.UseModel(model);
            builder.UseLoggerFactory(loggerFactory ?? _loggerFactory);

            return new BaseDbContext(builder.Options, conString, dbType);
        }

        private static ILoggerFactory _loggerFactory =
            new LoggerFactory(new ILoggerProvider[] { new EFCoreSqlLogeerProvider() });

        #endregion
    }
}
