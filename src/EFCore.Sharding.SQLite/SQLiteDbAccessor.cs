namespace EFCore.Sharding.SQLite
{
    internal class SQLiteDbAccessor : GenericDbAccessor, IDbAccessor
    {
        public SQLiteDbAccessor(GenericDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"\"{name}\"";
        }

        protected override string GetSchema(string schema)
        {
            return null;
        }
    }
}
