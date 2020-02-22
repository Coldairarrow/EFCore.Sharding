using EFCore.Sharding.Util;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public static class DbFactory
    {
        #region 外部接口

        /// <summary>
        /// 根据配置文件获取数据库类型，并返回对应的工厂接口
        /// </summary>
        /// <param name="conString">完整数据库链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static IRepository GetRepository(string conString, DatabaseType dbType)
        {
            Type dbRepositoryType = Type.GetType("EFCore.Sharding." + DbProviderFactoryHelper.DbTypeToDbTypeStr(dbType) + "Repository");

            var repository = Activator.CreateInstance(dbRepositoryType, new object[] { conString }) as IRepository;

            return repository;
        }

        /// <summary>
        /// 获取ShardingRepository
        /// </summary>
        /// <param name="absDbName">抽象数据库</param>
        /// <returns>ShardingRepository</returns>
        public static IShardingRepository GetShardingRepository(string absDbName = ShardingConfig.DefaultAbsDbName)
        {
            ShardingConfig.CheckInit();

            return new ShardingRepository(GetRepository("DataSource=db.db", DatabaseType.SQLite), absDbName);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="conString"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        internal static BaseDbContext GetDbContext([NotNull] string conString, DatabaseType dbType)
        {
            if (conString.IsNullOrEmpty())
                throw new Exception("conString能为空");

            DbConnection dbConnection = null;
            if (dbType != DatabaseType.Memory)
                dbConnection = DbProviderFactoryHelper.GetDbConnection(conString, dbType);
            var model = DbModelFactory.GetDbCompiledModel(conString, dbType);
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();

            switch (dbType)
            {
                case DatabaseType.SqlServer: builder.UseSqlServer(dbConnection, x => x.UseRowNumberForPaging()); break;
                case DatabaseType.MySql: builder.UseMySql(dbConnection); break;
                case DatabaseType.PostgreSql: builder.UseNpgsql(dbConnection); break;
                case DatabaseType.Oracle: builder.UseOracle(dbConnection, x => x.UseOracleSQLCompatibility("11")); break;
                case DatabaseType.SQLite: builder.UseSqlite(dbConnection); break;
                case DatabaseType.Memory: builder.UseInMemoryDatabase(conString); break;
                default: throw new Exception("暂不支持该数据库！");
            }
            builder.EnableSensitiveDataLogging();
            builder.UseModel(model);
            builder.UseLoggerFactory(_loggerFactory);

            return new BaseDbContext(builder.Options);
        }

        private static ILoggerFactory _loggerFactory =
            new LoggerFactory(new ILoggerProvider[] { new EFCoreSqlLogeerProvider() });

        #endregion
    }
}
