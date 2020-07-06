using System;

namespace EFCore.Sharding
{
    internal static partial class Extention
    {
        public static bool IsNullable(this Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition() == (typeof(Nullable<>)));
        }
    }
}
