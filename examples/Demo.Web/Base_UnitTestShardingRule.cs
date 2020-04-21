using Demo.Common;
using EFCore.Sharding;

namespace Demo.Web
{
    class Base_UnitTestShardingRule : ModShardingRule<Base_UnitTest>
    {
        protected override string KeyField => "Id";
        protected override int Mod => 3;
    }
}
