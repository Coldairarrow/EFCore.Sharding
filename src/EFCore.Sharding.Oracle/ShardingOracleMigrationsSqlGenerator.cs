﻿using EFCore.Sharding.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Oracle.EntityFrameworkCore.Infrastructure.Internal;
using Oracle.EntityFrameworkCore.Migrations;
using System.Linq;

namespace EFCore.Sharding.Oracle
{
    internal class ShardingOracleMigrationsSqlGenerator : OracleMigrationsSqlGenerator
    {
        public ShardingOracleMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IOracleOptions options) : base(dependencies, options)
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
