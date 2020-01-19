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
            //var assemblys = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //assemblys.RemoveAll(x => x.IsDynamic);
            //List<Type> allTypes = new List<Type>();
            //assemblys.ForEach(aAssembly =>
            //{
            //    Assembly.Load(aAssembly.FullName);
            //    allTypes.AddRange(aAssembly.GetTypes());
            //});

            //FxAllTypes = allTypes;

            FxAllTypes = Assembly
               .GetEntryAssembly()
               .GetReferencedAssemblies()
               .Select(Assembly.Load)
               .SelectMany(x => x.DefinedTypes)
               .Select(x => x as Type)
               .ToList();
        }

        /// <summary>
        /// 框架所有自定义类
        /// </summary>
        public static readonly List<Type> FxAllTypes;
    }
}
