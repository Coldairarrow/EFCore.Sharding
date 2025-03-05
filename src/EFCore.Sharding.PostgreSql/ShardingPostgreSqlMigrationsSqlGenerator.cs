using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using System.Linq;

namespace EFCore.Sharding.PostgreSql
{
    internal class ShardingPostgreSqlMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
#if NET6_0_OR_GREATER
        public ShardingPostgreSqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlSingletonOptions) : base(dependencies, npgsqlSingletonOptions)
        {
        }
#else
        public ShardingPostgreSqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlOptions npgsqlOptions) : base(dependencies, npgsqlOptions)

        {
        }
#endif

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            System.Collections.Generic.List<MigrationCommand> oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            System.Collections.Generic.List<MigrationCommand> newCmds = builder.GetCommandList().ToList();
            System.Collections.Generic.List<MigrationCommand> addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }
    }
}
