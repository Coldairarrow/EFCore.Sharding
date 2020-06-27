using EFCore.Sharding.Util;
using System;
using System.Collections.Generic;

namespace EFCore.Sharding.PostgreSql
{
    internal class PostgreSqlDbAccessor : AbstractDbAccessor, IDbAccessor
    {
        public PostgreSqlDbAccessor(BaseDbContext baseDbContext)
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

        protected override string GetSchema(string schema)
        {
            if (schema.IsNullOrEmpty())
                return "public";
            else
                return schema;
        }
    }
}
