using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Linq;

namespace EFCore.Sharding.SQLite
{
    internal class ShardingSQLiteMigrationsSqlGenerator : SqliteMigrationsSqlGenerator
    {
        public ShardingSQLiteMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }

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
