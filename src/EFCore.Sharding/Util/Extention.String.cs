using Newtonsoft.Json;
using System;
using System.Data;

namespace EFCore.Sharding
{
    internal static partial class Extention
    {
        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static T ToObject<T>(this string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static object ToObject(this string jsonStr, Type type)
        {
            return JsonConvert.DeserializeObject(jsonStr, type);
        }

        /// <summary>
        /// 将Json字符串转为DataTable
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this string jsonStr)
        {
            return jsonStr == null ? null : JsonConvert.DeserializeObject<DataTable>(jsonStr);
        }
    }
}
