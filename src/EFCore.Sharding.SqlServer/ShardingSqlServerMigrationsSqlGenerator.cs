using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using System.Linq;

namespace EFCore.Sharding.SqlServer
{
    internal class ShardingSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {

#if NET9_0 || NET8_0 ||NET7_0
        public ShardingSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer) : base(dependencies, commandBatchPreparer)
        {
        }
#endif
#if NET6_0 || NETSTANDARD2_1
        public ShardingSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
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
