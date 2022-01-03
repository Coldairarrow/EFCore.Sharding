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
#pragma warning disable EF1001 // Internal EF Core API usage.
        public ShardingPostgreSqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,  INpgsqlOptions npgsqlOptions) : base(dependencies, npgsqlOptions)
#pragma warning restore EF1001 // Internal EF Core API usage.
        {
        }

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            var oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            var newCmds = builder.GetCommandList().ToList();
            var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }
    }
}
