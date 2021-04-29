using Castle.DynamicProxy;
using Dynamitey;
using System;
using System.ComponentModel;
using System.Reflection;

namespace EFCore.Sharding
{
    internal static partial class Extention
    {
        private static readonly BindingFlags _bindingFlags
            = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        /// <summary>
        /// 判断是否为Null或者空
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            if (obj == null)
                return true;
            else
            {
                string objStr = obj.ToString();
                return string.IsNullOrEmpty(objStr);
            }
        }

        /// <summary>
        /// 获取某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName, _bindingFlags);
            if (property != null)
            {
                return obj.GetType().GetProperty(propertyName, _bindingFlags).GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 设置某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static object GetGetFieldValue(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, _bindingFlags).GetValue(obj);
        }

        /// <summary>
        /// 设置某字段值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 改变类型
        /// </summary>
        /// <param name="obj">原对象</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static object ChangeType_ByConvert(this object obj, Type targetType)
        {
            object resObj;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter newNullableConverter = new NullableConverter(targetType);
                resObj = newNullableConverter.ConvertFrom(obj);
            }
            else
            {
                resObj = Convert.ChangeType(obj, targetType);
            }

            return resObj;
        }

        /// <summary>
        /// 生成代理
        /// </summary>
        /// <typeparam name="T">代理类型</typeparam>
        /// <param name="obj">实际类型</param>
        /// <returns></returns>
        public static T ActLike<T>(this object obj) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new ActLikeInterceptor(obj));
        }

        private class ActLikeInterceptor : IInterceptor
        {
            public ActLikeInterceptor(object obj)
            {
                _obj = obj;
            }
            private object _obj;
            public void Intercept(IInvocation invocation)
            {
                var method = invocation.Method;

                //属性处理
                if (method.Name.StartsWith("get_"))
                {
                    invocation.ReturnValue = Dynamic.InvokeGet(_obj, method.Name.Substring(4));

                    return;
                }
                else if (method.Name.StartsWith("set_"))
                {
                    Dynamic.InvokeSet(_obj, method.Name.Substring(4), invocation.Arguments[0]);

                    return;
                }

                //方法处理
                var name = new InvokeMemberName(method.Name, method.GetGenericArguments());
                if (invocation.Method.ReturnType != typeof(void))
                {
                    invocation.ReturnValue = Dynamic.InvokeMember(_obj, name, invocation.Arguments);
                }
                else
                {
                    Dynamic.InvokeMemberAction(_obj, name, invocation.Arguments);
                }
            }
        }
    }
}
