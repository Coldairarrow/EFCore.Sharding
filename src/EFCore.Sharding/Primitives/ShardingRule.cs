using System;

namespace EFCore.Sharding
{
    internal class ShardingRule
    {
        public Type EntityType { get; set; }
        public string TableName { get => AnnotationHelper.GetDbTableName(EntityType); }
        public ShardingType ShardingType { get; set; }
        public string ShardingField { get; set; }
        public int Mod { get; set; }
        public ExpandByDateMode? ExpandByDateMode { get; set; }
        public string GetTableSuffixByField(object fieldValue)
        {
            switch (ShardingType)
            {
                case ShardingType.HashMod:
                    {
                        long suffix;
                        if (fieldValue.GetType() == typeof(int) || fieldValue.GetType() == typeof(long))
                        {
                            long longValue = (long)fieldValue;
                            if (longValue < 0)
                                throw new Exception($"字段{ShardingField}不能小于0");

                            suffix = longValue % Mod;
                        }
                        else
                        {
                            suffix = Math.Abs(fieldValue.GetHashCode()) % Mod;
                        }

                        return suffix.ToString();
                    };
                case ShardingType.Date:
                    {
                        string format = ExpandByDateMode switch
                        {
                            Sharding.ExpandByDateMode.PerMinute => "yyyyMMddHHmm",
                            Sharding.ExpandByDateMode.PerHour => "yyyyMMddHH",
                            Sharding.ExpandByDateMode.PerDay => "yyyyMMdd",
                            Sharding.ExpandByDateMode.PerMonth => "yyyyMM",
                            Sharding.ExpandByDateMode.PerYear => "yyyy",
                            _ => throw new Exception("ExpandByDateMode无效")
                        };

                        return ((DateTime)fieldValue).ToString(format);
                    };
                default: throw new Exception("ShardingType无效");
            }
        }
        public string GetTableSuffixByEntity(object entity)
        {
            var property = entity.GetPropertyValue(ShardingField);

            return GetTableSuffixByField(property);
        }
    }
}
