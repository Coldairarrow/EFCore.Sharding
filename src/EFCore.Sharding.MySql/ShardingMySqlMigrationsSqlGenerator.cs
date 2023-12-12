using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EFCore.Sharding.MySql
{
    internal class ShardingMySqlMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
    {
#if NET8_0
        public ShardingMySqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer, IMySqlOptions options) : base(dependencies, commandBatchPreparer, options)
        {
        }
#endif
#if NET7_0
        public ShardingMySqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer, IMySqlOptions options) : base(dependencies, commandBatchPreparer, options)
        {
        }
#endif
#if NET6_0
        public ShardingMySqlMigrationsSqlGenerator([NotNullAttribute] MigrationsSqlGeneratorDependencies dependencies, [NotNullAttribute] IRelationalAnnotationProvider annotationProvider, [NotNullAttribute] IMySqlOptions options) : base(dependencies, annotationProvider, options)
        {
        }
#endif
#if NETSTANDARD2_1
        public ShardingMySqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider annotationProvider, IMySqlOptions options) : base(dependencies, annotationProvider, options)
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
