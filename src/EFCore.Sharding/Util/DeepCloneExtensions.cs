using System;
using System.Collections.Generic;
using System.Reflection;

namespace EFCore.Sharding
{
    internal static class DeepCloneExtensions
    {
        public static T DeepClone<T>(this T original)
        {
            return (T)DeepClone((object)original);
        }
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        private static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }
        private static object DeepClone(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }
        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    ArrayExtensions.ForEach(clonedArray, (array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }
            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }
        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }
        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        private class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }
            public override int GetHashCode(object obj)
            {
                if (obj == null) return 0;
                return obj.GetHashCode();
            }
        }
        private static class ArrayExtensions
        {
            public static void ForEach(Array array, Action<Array, int[]> action)
            {
                if (array.LongLength == 0) return;
                ArrayTraverse walker = new ArrayTraverse(array);
                do action(array, walker.Position);
                while (walker.Step());
            }
        }

        internal class ArrayTraverse
        {
            public int[] Position;
            private int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                {
                    maxLengths[i] = array.GetLength(i) - 1;
                }
                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] < maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }
}