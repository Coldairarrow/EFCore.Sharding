using EFCore.Sharding.Util;

namespace EFCore.Sharding
{
    /// <summary>
    /// 取模分片规则
    /// 说明:根据某字段的HASH,然后取模后得到表名后缀
    /// 举例:Base_User_0,Base_User为抽象表名,_0为后缀
    /// 警告:使用简单,但是扩容后需要大量数据迁移,不推荐使用
    /// </summary>
    public abstract class ModShardingRule<TEntity> : AbsShardingRule<TEntity>
    {
        /// <summary>
        /// 用来mod的字段
        /// </summary>
        protected abstract string KeyField { get; }

        /// <summary>
        /// mod值
        /// </summary>
        protected abstract int Mod { get; }

        /// <summary>
        /// 生成表名后缀(推荐实现此方法)
        /// 注:若逻辑表为Base_UnitTest,生成的后缀为1,则最终确定的表名为Base_UnitTest_1
        /// 注:BuildTableSuffix与BuildTableName实现二选一
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>
        /// 表名后缀
        /// </returns>
        public override string BuildTableSuffix(TEntity obj)
        {
            var suffix = obj.GetPropertyValue(KeyField).ToString().ToMurmurHash() % Mod;
            return suffix.ToString();
        }
    }
}
