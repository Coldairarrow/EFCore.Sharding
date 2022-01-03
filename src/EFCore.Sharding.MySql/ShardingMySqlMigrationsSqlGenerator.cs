using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using System.Linq;

namespace EFCore.Sharding.MySql
{
    internal class ShardingMySqlMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        public ShardingMySqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider annotationProvider, IMySqlOptions options) : base(dependencies, annotationProvider, options)
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
