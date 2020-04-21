using Demo.Common;
using EFCore.Sharding;

namespace Demo.Web
{
    class Base_UnitTest_LongKeyShardingRule : ModShardingRule<Base_UnitTest_LongKey>
    {
        protected override string KeyField => "Id";
        protected override int Mod => 3;
    }
}
