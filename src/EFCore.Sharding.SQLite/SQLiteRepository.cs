using System;
using System.Collections.Generic;

namespace EFCore.Sharding.SQLite
{
    internal class SQLiteRepository : DbRepository, IRepository
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        public SQLiteRepository(string conStr)
            : base(conStr, DatabaseType.SQLite)
        {
        }

        public override void BulkInsert<T>(List<T> entities)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 私有成员

        protected override string FormatFieldName(string name)
        {
            return $"[{name}]";
        }

        #endregion

        #region 插入数据

        #endregion

        #region 删除数据

        #endregion
    }
}
