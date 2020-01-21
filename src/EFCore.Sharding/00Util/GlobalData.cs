using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Sharding.Util
{
    internal static class GlobalData
    {
        static GlobalData()
        {
            AppDomain.CurrentDomain.GetAssemblies().Where(x =>
                !x.FullName.Contains("System")
                && !x.FullName.Contains("Microsoft")
                && !x.IsDynamic)
                .ForEach(aAssembly =>
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
