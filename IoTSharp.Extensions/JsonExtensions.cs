using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace IoTSharp.Extensions
{
    public static class JsonExtensions
    {
        public static T ToJson<T>(this IDataReader dataReader) where T : class
        {
            return dataReader.ToJson().ToObject<T>();
        }
        public static List<T> ToList<T>(this IDataReader dataReader) where T : class
        {
            return dataReader.ToJson().ToObject<List<T>>();
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

    }
}
