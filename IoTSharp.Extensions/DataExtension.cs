using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Uixe.Extensions
{
    public static class DataExtension
    {
      

        public static string ToISO8601(this System.DateTime time) => time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:sssZ");

        public static T ToJson<T>(this IDataReader dataReader) where T : class
        {
            return dataReader.ToJson().ToObject<T>();
        }

        

        public static JArray ToJson(this IDataReader dataReader)
        {
            JArray jArray = new JArray();
            try
            {
                while (dataReader.Read())
                {
                    JObject jObject = new JObject();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        try
                        {
                            string strKey = dataReader.GetName(i);
                            if (dataReader[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dataReader[i], dataReader.GetFieldType(i));
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


        public static List<T> ToList<T>(this IDataReader dataReader) where T : class
        {
            List<T> jArray = new List<T>();
            var prs = typeof(T).GetProperties();
            try
            {
                while (dataReader.Read())
                {
                    T jObject = Activator.CreateInstance<T>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        try
                        {
                            string strKey = dataReader.GetName(i);
                          
                            if (dataReader[i] != DBNull.Value)
                            {
                                var ft = dataReader.GetFieldType(i);
                                var _v = dataReader[i];
                                object obj = Convert.ChangeType(_v, ft);
                                var p = prs.FirstOrDefault(px => px.Name.ToLower() == strKey.ToLower());
                                if (p != null)
                                {
                                    SetValue(jObject, ft, obj, p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
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
    }
}