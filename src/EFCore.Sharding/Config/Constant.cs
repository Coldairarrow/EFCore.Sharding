using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Sharding
{
    internal static class Constant
    {
        public static int CommandTimeout { get; set; } = 30;
        public static bool LogicDelete { get; set; } = false;
        public static string KeyField { get; set; } = "Id";
        public static string DeletedField { get; set; } = "Deleted";
        public static List<Type> AllEntityTypes
        {
            get
            {
                if (_allEntityTypes == null)
                {
                    lock (_entityLock)
                    {
                        if (_allEntityTypes == null)
                        {
                            _allEntityTypes = new List<Type>();

                            Expression<Func<string, bool>> where = x => true;
                            where = where.And(x =>
                                  !x.Contains("System.")
                                  && !x.Contains("Microsoft."));
                            if (AssemblyNames.Count > 0)
                            {
                                Expression<Func<string, bool>> tmpWhere = x => false;
                                AssemblyNames.ToList().ForEach(aAssembly =>
                                {
                                    tmpWhere = tmpWhere.Or(x => x.Contains(aAssembly));
                                });

                                where = where.And(tmpWhere);
                            }

                            AssemblyPaths.SelectMany(x => Directory.GetFiles(x, "*.dll"))
                                .Where(x => where.Compile()(new FileInfo(x).Name))
                                .Distinct()
                                .Select(x =>
                                {
                                    try
                                    {
                                        return Assembly.LoadFrom(x);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                })
                                .Where(x => x != null && !x.IsDynamic)
                                .ForEach(aAssembly =>
                                {
                                    try
                                    {
                                        Assemblies.Add(aAssembly);
                                        _allEntityTypes.AddRange(aAssembly.GetTypes());
                                    }
                                    catch
                                    {

                                    }
                                });
                        }
                    }
                }

                return _allEntityTypes;
            }
        }
        public static List<string> AssemblyNames = new List<string>();
        public static readonly List<Assembly> Assemblies = new List<Assembly>();
        public static List<string> AssemblyPaths
            = new List<string>() { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };
        private static List<Type> _allEntityTypes;
        private static object _entityLock = new object();
    }
}
