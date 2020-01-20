using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EFCore.Sharding.Util
{
    internal static class GlobalData
    {
        static GlobalData()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
               //.Select(Assembly.Load)
               .SelectMany(x => x.DefinedTypes)
               .Select(x => x as Type)
               .ToList();
            var theType = types.Where(x => x.Name == "Base_UnitTest").ToList();
            FxAllTypes = types;
        }

        /// <summary>
        /// 框架所有自定义类
        /// </summary>
        public static readonly List<Type> FxAllTypes;
    }
}
