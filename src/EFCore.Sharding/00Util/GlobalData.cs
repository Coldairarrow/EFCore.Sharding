using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding.Util
{
    internal static class GlobalData
    {
        static GlobalData()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.RemoveAll(x =>
                x.FullName.Contains("System")
                || x.FullName.Contains("Microsoft")
                || x.IsDynamic);
            assemblies.ForEach(aAssembly =>
            {
                try
                {
                    FxAllTypes.AddRange(aAssembly.GetTypes());
                }
                catch
                {

                }
            });
        }

        /// <summary>
        /// 框架所有自定义类
        /// </summary>
        public static readonly List<Type> FxAllTypes = new List<Type>();
    }
}
