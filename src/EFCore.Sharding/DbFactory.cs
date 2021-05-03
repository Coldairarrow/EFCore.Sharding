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
        private readonly IOptionsSnapshot<EFCoreShardingOptions> _optionsSnapshot;
        public DbFactory(ILoggerFactory loggerFactory, IOptionsSnapshot<EFCoreShardingOptions> optionsSnapshot)
        {
            _loggerFactory = loggerFactory;
            _optionsSnapshot = optionsSnapshot;
        }

        public void CreateTable(string conString, DatabaseType dbType, Type entityType, string suffix)
        {
            DbContextParamters options = new DbContextParamters
            {
                ConnectionString = conString,
                DbType = dbType,
                EntityTypes = new Type[] { entityType },
                Suffix = suffix
            };

            using DbContext dbContext = GetDbContext(options, _optionsSnapshot.BuildOption(null));
            var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            try
            {
                databaseCreator.CreateTables();
            }
            catch
            {

            }
        }

        public IDbAccessor GetDbAccessor(DbContextParamters dbContextParamters, string optionName = null)
        {
            EFCoreShardingOptions eFCoreShardingOptions = _optionsSnapshot.BuildOption(optionName);

            var dbContext = GetDbContext(dbContextParamters, eFCoreShardingOptions);

            return GetProvider(dbContextParamters.DbType).GetDbAccessor(dbContext);
        }

        public GenericDbContext GetDbContext(DbContextParamters dbContextParamters, EFCoreShardingOptions eFCoreShardingOptions)
        {
            if (eFCoreShardingOptions == null)
            {
                eFCoreShardingOptions = _optionsSnapshot.BuildOption(null);
            }

            AbstractProvider provider = GetProvider(dbContextParamters.DbType);

            DbConnection dbConnection = provider.GetDbConnection();
            dbConnection.ConnectionString = dbContextParamters.ConnectionString;

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseLoggerFactory(_loggerFactory);

            provider.UseDatabase(builder, dbConnection);
            builder.ReplaceService<IModelCacheKeyFactory, GenericModelCacheKeyFactory>();
            builder.ReplaceService<IMigrationsModelDiffer, ShardingMigration>();

            return new GenericDbContext(builder.Options, dbContextParamters, eFCoreShardingOptions);
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
