using System;
using System.Collections.Generic;

namespace EFCore.Sharding.Oracle
{
    internal class OracleRepository : DbRepository, IRepository
    {
        public OracleRepository(BaseDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"\"{name}\"";
        }

        protected override string FormatParamterName(string name)
        {
            return $":{name}";
        }

        public override void BulkInsert<T>(List<T> entities)
        {
            throw new Exception("暂不支持");
        }
    }
}
