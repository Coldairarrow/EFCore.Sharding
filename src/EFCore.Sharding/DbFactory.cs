using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Reflection;

namespace EFCore.Sharding
{
    internal class DbFactory : IDbFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly EFCoreShardingOptions _shardingOptions;
        public DbFactory(ILoggerFactory loggerFactory, IOptions<EFCoreShardingOptions> shardingOptions)
        {
            _loggerFactory = loggerFactory;
            _shardingOptions = shardingOptions.Value;
        }
        public IDbAccessor GetDbAccessor(string conString, DatabaseType dbType, string entityNamespace = null, string suffix = null)
        {
            GenericDbContextOptions options = new GenericDbContextOptions
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityNamespace = entityNamespace,
                Suffix = suffix,
            };

            var dbContext = GetDbContext(options);

            return GetProvider(dbType).GetDbAccessor(dbContext as GenericDbContext);
        }
        public void CreateTable(string conString, DatabaseType dbType, Type entityType, string suffix)
        {
            GenericDbContextOptions options = new GenericDbContextOptions
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityTypes = new Type[] { entityType },
                Suffix = suffix
            };

            using DbContext dbContext = GetDbContext(options);
            var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            try
            {
                databaseCreator.CreateTables();
            }
            catch
            {

            }
        }
        public DbContext GetDbContext(GenericDbContextOptions options)
        {
            if (options.ConnectionString.IsNullOrEmpty())
            {
                throw new Exception("连接字符串不能为空");
            }

            AbstractProvider provider = GetProvider(options.DbType);

            DbConnection dbConnection = provider.GetDbConnection();
            dbConnection.ConnectionString = options.ConnectionString;

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();

            provider.UseDatabase(builder, dbConnection);
            builder.ReplaceService<IModelCacheKeyFactory, GenericModelCacheKeyFactory>();
#if EFCORE3
            if (_shardingOptions.MigrationsWithoutForeignKey)
            {
                builder.ReplaceService<IMigrationsModelDiffer, MigrationsWithoutForeignKey>();
            }
#endif
            builder.UseLoggerFactory(_loggerFactory);

            return new GenericDbContext(builder.Options, options, _shardingOptions);
        }
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
    }
}
