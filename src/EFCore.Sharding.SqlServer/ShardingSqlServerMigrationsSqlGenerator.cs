using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Linq;

namespace EFCore.Sharding.SqlServer
{
    internal class ShardingSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
#if EFCORE5
        public ShardingSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }
#endif
#if EFCORE3
        public ShardingSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations)
            : base(dependencies, migrationsAnnotations)
        {
        }
#endif
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
