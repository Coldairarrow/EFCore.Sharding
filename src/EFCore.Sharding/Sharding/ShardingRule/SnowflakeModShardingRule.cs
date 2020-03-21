using Coldairarrow.Util;
using EFCore.Sharding.Util;
using System;

namespace EFCore.Sharding
{
    /// <summary>
    /// 基于雪花Id的mod分片,具体的规则请参考此本写法
    /// 优点:数据扩容无需数据迁移,以时间轴进行扩容
    /// 缺点:可能会存在数据热点问题
    /// 建议:推荐使用此分片规则,易于使用
    /// </summary>
    public class SnowflakeModShardingRule<TEntity> : AbsShardingRule<TEntity>
    {
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
            //主键Id必须为SnowflakeId
            SnowflakeId snowflakeId = new SnowflakeId((long)obj.GetPropertyValue("Id"));
            //2019-5-10之前mod3
            if (snowflakeId.Time < DateTime.Parse("2019-5-10"))
                return (snowflakeId.Id.GetHashCode() % 3).ToString();
            //2019-5-10之后mod10
            if (snowflakeId.Time >= DateTime.Parse("2019-5-10"))
                return (snowflakeId.Id.GetHashCode() % 10).ToString();
            //以此类推balabala

            throw new NotImplementedException();
        }
    }
}
