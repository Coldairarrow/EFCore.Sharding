using Microsoft.EntityFrameworkCore;

namespace EFCore.Sharding
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public interface IDbFactory
    {
        /// <summary>
        /// 根据配置文件获取数据库类型，并返回对应的工厂接口
        /// </summary>
        /// <param name="conString">完整数据库链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="suffix">表明后缀</param>
        /// <returns></returns>
        IDbAccessor GetDbAccessor(string conString, DatabaseType dbType, string entityNamespace = null, string suffix = null);

        /// <summary>
        /// 获取DbContext
        /// </summary>
        /// <param name="options">选项</param>
        /// <returns></returns>
        DbContext GetDbContext(GenericDbContextOptions options);
    }
}
