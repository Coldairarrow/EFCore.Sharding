using Microsoft.Extensions.Options;

namespace EFCore.Sharding
{
    internal static class OptionExtentions
    {
        public static EFCoreShardingOptions BuildOption(this IOptionsMonitor<EFCoreShardingOptions> optionsSnapshot, string optionName)
        {
            if (optionName.IsNullOrEmpty())
            {
                return optionsSnapshot.CurrentValue;
            }
            else
            {
                //var selfOption = optionsSnapshot.Get(optionName).DeepClone();
                EFCoreShardingOptions selfOption = optionsSnapshot.Get(optionName);
                EFCoreShardingOptions defaultOption = new();
                EFCoreShardingOptions globalOption = optionsSnapshot.CurrentValue;

                foreach (System.Reflection.PropertyInfo aProperty in typeof(EFCoreShardingOptions).GetProperties())
                {
                    object selfValue = aProperty.GetValue(selfOption);
                    object defaultValue = aProperty.GetValue(defaultOption);
                    object globalValue = aProperty.GetValue(globalOption);

                    object value = Equals(selfValue, defaultValue) ? globalValue : selfValue;
                    aProperty.SetValue(selfOption, value);
                }

                return selfOption;
            }
        }
    }
}
