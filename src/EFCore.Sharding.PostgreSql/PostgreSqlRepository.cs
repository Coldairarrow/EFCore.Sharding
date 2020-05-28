using System;
using System.Collections.Generic;

namespace EFCore.Sharding.PostgreSql
{
    internal class PostgreSqlRepository : DbRepository, IRepository
    {
        public PostgreSqlRepository(BaseDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"\"{name}\"";
        }

        public override void BulkInsert<T>(List<T> entities)
        {
            throw new Exception("抱歉！暂不支持PostgreSql！");
        }
    }
}
