using EFCore.Sharding.Util;

namespace EFCore.Sharding
{
    /// <summary>
    /// 取模分片规则
    /// 说明:根据某字段的HASH,然后取模后得到表名后缀
    /// 举例:Base_User_0,Base_User为抽象表名,_0为后缀
    /// 警告:使用简单,但是扩容后需要大量数据迁移,不推荐使用
    /// </summary>
    public abstract class ModShardingRule<T> : IShardingRule<T>
    {
        protected abstract string KeyField { get; }
        protected abstract int Mod { get; }
        public string FindTable(T obj)
        {
            return $"{typeof(T).Name}_{(uint)(obj.GetPropertyValue(KeyField).ToString().ToMurmurHash() % Mod)}";
        }
    }
}
