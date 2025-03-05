namespace EFCore.Sharding.PostgreSql
{
    internal class PostgreSqlDbAccessor : GenericDbAccessor, IDbAccessor
    {
        public PostgreSqlDbAccessor(GenericDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"\"{name}\"";
        }

        protected override string GetSchema(string schema)
        {
            return schema.IsNullOrEmpty() ? "public" : schema;
        }
    }
}
