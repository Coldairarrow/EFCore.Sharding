using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

using System.Linq;

namespace EFCore.Sharding.Oracle
{
    internal class ShardingOracleMigrationsSqlGenerator : OracleMigrationsSqlGenerator
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        public ShardingOracleMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies) : base(dependencies)
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
