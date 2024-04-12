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
                var selfOption = optionsSnapshot.Get(optionName);
                var defaultOption = new EFCoreShardingOptions();
                var globalOption = optionsSnapshot.CurrentValue;

                foreach (var aProperty in typeof(EFCoreShardingOptions).GetProperties())
                {
                    var selfValue = aProperty.GetValue(selfOption);
                    var defaultValue = aProperty.GetValue(defaultOption);
                    var globalValue = aProperty.GetValue(globalOption);

                    var value = Equals(selfValue, defaultValue) ? globalValue : selfValue;
                    aProperty.SetValue(selfOption, value);
                }

                return selfOption;
            }
        }
    }
}
