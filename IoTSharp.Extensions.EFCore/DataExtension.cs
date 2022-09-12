using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data
{
    public static class DataExtension
    {
        public static IList<T> ToIList<T>(this DataTable dt) where T : class => dt.ToList<T>();

        public static List<T> ToList<T>(this DataTable dt) where T : class
        {
            List<T> jArray = new List<T>();
            var prs = typeof(T).GetProperties();
            try
            {
                for (int il = 0; il < dt.Rows.Count; il++)
                {
                    T jObject = Activator.CreateInstance<T>();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        try
                        {
                            string strKey = dt.Columns[i].ColumnName;
                            if (dt.Rows[il].ItemArray[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dt.Rows[il].ItemArray[i], dt.Columns[i].DataType);

                                var p = prs.FirstOrDefault(px => px.Name.ToLower() == strKey.ToLower());
                                if (p != null)
                                {
                                    SetValue(jObject, dt.Columns[i].DataType, obj, p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return jArray;
        }

        /// <summary>
        /// DataTable 转为 Json
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static JArray ToJson(this DataTable dt)
        {
            JArray jArray = new JArray();
            try
            {
                for (int il = 0; il < dt.Rows.Count; il++)
                {
                    JObject jObject = new JObject();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        try
                        {
                            string strKey = dt.Columns[i].ColumnName;
                            if (dt.Rows[il].ItemArray[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dt.Rows[il].ItemArray[i], dt.Columns[i].DataType);
                                jObject.Add(strKey, JToken.FromObject(obj));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return jArray;
        }

        private static void SetValue<T>(T jObject, Type ft, object obj, System.Reflection.FieldInfo p)
        {
            if (p.FieldType == ft)
            {
                p.SetValue(jObject, obj);
            }
            else if (p.FieldType == typeof(DateTime) && ft == typeof(string))
            {
                if (DateTime.TryParse((string)obj, out DateTime dt))
                {
                    p.SetValue(jObject, dt);
                }
            }
            else
            {
                p.SetValue(jObject, Convert.ChangeType(obj, p.FieldType));
            }
        }

        private static void SetValue<T>(T jObject, Type ft, object obj, System.Reflection.PropertyInfo p) where T : class
        {
            if (p.PropertyType == ft)
            {
                p.SetValue(jObject, obj);
            }
            else if (p.PropertyType == typeof(DateTime) && ft == typeof(string))
            {
                if (DateTime.TryParse((string)obj, out DateTime dt))
                {
                    p.SetValue(jObject, dt);
                }
            }
            else
            {
                p.SetValue(jObject, Convert.ChangeType(obj, p.PropertyType));
            }
        }

        /// <summary>
        ///将DataTable转换为标准的CSV字符串
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <returns>返回标准的CSV</returns>
        /// <seealso cref="https://github.com/Coldairarrow/EFCore.Sharding/blob/5216ffc6330e84de484865eae645c0be1f2be180/src/EFCore.Sharding.Tests/Util/Extention.DataTable.cs"/>
        public static string ToCsvStr(this DataTable dt)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    colum = dt.Columns[i];
                    if (i != 0) sb.Append(",");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(row[colum].ToString());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}