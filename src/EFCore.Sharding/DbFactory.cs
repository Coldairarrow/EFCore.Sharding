using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Reflection;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    internal static class DbFactory
    {
        #region 外部接口

        public static AbstractProvider GetProvider(DatabaseType databaseType)
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
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="suffix">表明后缀</param>
        /// <returns></returns>
        public static IDbAccessor GetDbAccessor(string conString, DatabaseType dbType, string entityNamespace = null, ILoggerFactory loggerFactory = null, string suffix = null)
        {
            GenericDbContextOptions options = new GenericDbContextOptions
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityNamespace = entityNamespace,
                LoggerFactory = loggerFactory,
                Suffix = suffix
            };

            var dbContext = GetDbContext(options);

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

        public static void CreateTable(string conString, DatabaseType dbType, Type entityType, string suffix)
        {
            GenericDbContextOptions options = new GenericDbContextOptions
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityTypes = new Type[] { entityType },
                Suffix = suffix
            };

            using (DbContext dbContext = GetDbContext(options))
            {
                var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                try
                {
                    databaseCreator.CreateTables();
                }
                catch
                {

                }
            }
        }

        public static GenericDbContext GetDbContext(GenericDbContextOptions options)
        {
            AbstractProvider provider = GetProvider(options.DbType);

            DbConnection dbConnection = provider.GetDbConnection();
            dbConnection.ConnectionString = options.ConnectionString;

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();

            provider.UseDatabase(builder, dbConnection);
            builder.ReplaceService<IModelCacheKeyFactory, GenericModelCacheKeyFactory>();

            builder.EnableSensitiveDataLogging();
            builder.UseLoggerFactory(options.LoggerFactory ?? _loggerFactory);

            options.ContextOptions = builder.Options;

            return new GenericDbContext(options);
        }

        private static ILoggerFactory _loggerFactory =
            new LoggerFactory(new ILoggerProvider[] { new EFCoreSqlLogeerProvider() });

        #endregion
    }
}
