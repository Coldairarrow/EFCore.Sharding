using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Reflection;

namespace EFCore.Sharding
{
    internal class DbFactory : IDbFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IShardingConfig _shardingConfig;
        public DbFactory(ILoggerFactory loggerFactory, IShardingConfig shardingConfig)
        {
            _loggerFactory = loggerFactory;
            _shardingConfig = shardingConfig;
        }

        public IDbAccessor GetDbAccessor(string conString, DatabaseType dbType, string entityNamespace = null, string suffix = null)
        {
            GenericDbContextOptions options = new GenericDbContextOptions
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityNamespace = entityNamespace,
                LoggerFactory = _loggerFactory,
                Suffix = suffix,
                ShardingConfig = _shardingConfig
            };

            var dbContext = GetDbContext(options);

            return GetProvider(dbType).GetDbAccessor(dbContext);
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

            builder.UseLoggerFactory(options.LoggerFactory);

            options.ContextOptions = builder.Options;

            return new GenericDbContext(options);
        }
    }
}
