using EFCore.Sharding.Util;
using System.Collections.Generic;

namespace EFCore.Sharding
{
    /// <summary>
    /// 一致性HASH分片规则
    /// 优点:数据扩容时数据迁移量较小,表越多扩容效果越明显
    /// 缺点:扩容时需要进行数据迁移,比较复杂
    /// 建议:若雪花分片不满足则采用本方案,此方案为分片规则中的"核弹"
    /// </summary>
    public class ConsistentHashShardingRule<TEntity> : AbsShardingRule<TEntity>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tables">所有分表名集合</param>
        public ConsistentHashShardingRule(List<string> tables)
        {
            _tables = tables;
            _consistentHash.Init(tables);
        }

        /// <summary>
        /// 所有分表名集合
        /// </summary>
        protected List<string> _tables { get; }

        /// <summary>
        /// 一致性哈希
        /// </summary>
        protected ConsistentHash<string> _consistentHash { get; } = new ConsistentHash<string>();

        /// <summary>
        /// 生成完整表名
        /// 注:BuildTableSuffix与BuildTableName实现二选一
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>
        /// 完整表名
        /// </returns>
        public override string BuildTableName(TEntity obj)
        {
            string key = obj.GetPropertyValue("Id")?.ToString();

            return _consistentHash.GetNode(key);
        }
    }
}
