using EFCore.Sharding.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Sharding.Tests.DataAnnotations
{
    [TestClass]
    public class SqlDefaultTest : Startup
    {

        [TestMethod]
        public void SqlDefault()
        {
            var rep = ServiceProvider.GetService<IRepository>();

            var rep2 = rep as DbRepository;
            var databaseCreator = rep2.DbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

            var sql = databaseCreator.GenerateCreateScript();

            Console.WriteLine(sql);

            Assert.IsTrue(sql.Contains(@"CREATE TABLE ""sql_default_test"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_sql_default_test"" PRIMARY KEY AUTOINCREMENT,
    ""ModifiedOn"" TEXT NOT NULL DEFAULT (now())
);"));
        }

    }
}
