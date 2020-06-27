using System;
using System.Collections.Generic;

namespace EFCore.Sharding.SQLite
{
    internal class SQLiteDbAccessor : AbstractDbAccessor, IDbAccessor
    {
        public SQLiteDbAccessor(BaseDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        public override void BulkInsert<T>(List<T> entities)
        {
            throw new NotImplementedException();
        }

        protected override string FormatFieldName(string name)
        {
            return $"[{name}]";
        }

        protected override string GetSchema(string schema)
        {
            return null;
        }
    }
}
