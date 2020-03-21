using System;

namespace EFCore.Sharding
{
    /// <summary>
    /// 分表规则基类
    /// 注:具体分表规则只需要实现BuildTableSuffix,BuildTableName与BuildDate其中之一即可
    /// </summary>
    /// <typeparam name="TEntity">逻辑表泛型</typeparam>
    public abstract class AbsShardingRule<TEntity>
    {
        /// <summary>
        /// 生成表名后缀
        /// 注:若逻辑表为Base_UnitTest,生成的后缀为1,则最终确定的表名为Base_UnitTest_1
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>表名后缀</returns>
        public virtual string BuildTableSuffix(TEntity obj)
        {
            return string.Empty;
        }

        /// <summary>
        /// 生成完整表名
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>完整表名</returns>
        public virtual string BuildTableName(TEntity obj)
        {
            return typeof(TEntity).Name;
        }

        /// <summary>
        /// 生成日期
        /// 注:仅用在AutoExpandByDate中
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <returns>日期</returns>
        public virtual DateTime BuildDate(TEntity obj)
        {
            return DateTime.MinValue;
        }
    }
}
