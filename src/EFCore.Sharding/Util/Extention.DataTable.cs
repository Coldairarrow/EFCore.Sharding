﻿using System.Data;
using System.Text;

namespace EFCore.Sharding
{
    internal static partial class Extention
    {
        /// <summary>
        ///将DataTable转换为标准的CSV字符串
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <returns>返回标准的CSV</returns>
        public static string ToCsvStr(this DataTable dt)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new();
            DataColumn colum;
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    colum = dt.Columns[i];
                    if (i != 0)
                    {
                        _ = sb.Append(",");
                    }

                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        _ = sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else
                    {
                        _ = sb.Append(row[colum].ToString());
                    }
                }
                _ = sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
