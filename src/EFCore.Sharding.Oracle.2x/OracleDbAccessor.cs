using Microsoft.EntityFrameworkCore;

namespace EFCore.Sharding.Oracle
{
    internal class OracleDbAccessor : GenericDbAccessor, IDbAccessor
    {
        public OracleDbAccessor(GenericDbContext baseDbContext)
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

        protected override string GetSchema(string schema)
        {
            return _db.Database.GetDbConnection().Database;
        }
    }
}
