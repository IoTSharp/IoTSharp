using IoTSharp.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbReaderExtensions
    {
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
            finally
            {
                dataReader.Close();
            }
            return jArray;
        }

        public static IDbCommand CreateCommand(this IDbConnection db, string commandText)
        {
            var cmd = db.CreateCommand();
            cmd.CommandText = commandText;
            return cmd;
        }

        public static IDbCommand SetCommandTimeout(this IDbCommand command, TimeSpan span)
        {
            command.CommandTimeout = (int)span.TotalSeconds;
            return command;
        }

        public static IDictionary<string, DbColumn> GetSchema<T>(this IDataReader dr) => GetSchema<T>((DbDataReader)dr);

        public static IDictionary<string, DbColumn> GetSchema<T>(this DbDataReader dr)
        {
            IDictionary<string, DbColumn> valuePairs;
            if (typeof(T).IsTupleType())
            {
                var props = typeof(T).GetRuntimeFields();
                valuePairs = dr.GetColumnSchema()
               .ToDictionary(key => key.ColumnName.ToLower());
            }
            else
            {
                var props = typeof(T).GetRuntimeProperties();
                valuePairs = dr.GetColumnSchema()
               .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
               .ToDictionary(key => key.ColumnName.ToLower());
            }
            return valuePairs;
        }

        public static T MapObject<T>(this IDataReader dr, IDictionary<string, DbColumn> colMapping) => MapObject<T>((DbDataReader)dr, colMapping);

        public static T MapObject<T>(this DbDataReader dr, IDictionary<string, DbColumn> colMapping)
        {
            T t;
            if (typeof(T).IsSqlSimpleType())
            {
                t = (T)dr.GetValue(0);
            }
            else
            {
                T obj = Activator.CreateInstance<T>();
                if (typeof(T).IsTupleType())
                {
                    var fields = typeof(T).GetRuntimeFields().ToArray();
                    //https://stackoverflow.com/questions/59000557/valuetuple-set-fields-via-reflection
                    object xobj = obj;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        try
                        {
                            if (dr.GetValue(i) == DBNull.Value)
                            {
                                fields[i].SetValue(xobj, fields[i].FieldType.GetDefaultValue());
                            }
                            else
                            {
                                var val = Convert.ChangeType(dr.GetValue(i), fields[i].FieldType);
                                fields[i].SetValue(xobj, val);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{fields[i].Name} {ex.Message}");
                        }
                    }
                    obj = (T)xobj;
                }
                else
                {
                    IEnumerable<PropertyInfo> props = typeof(T).GetRuntimeProperties();
                    foreach (var prop in props)
                    {
                        var propName = prop.Name.ToLower();
                        if (colMapping.ContainsKey(propName))
                        {
                            var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);

                            var type = Nullable.GetUnderlyingType(prop.PropertyType);
                            if (type != null && type.IsEnum)
                            {
                                val = val == DBNull.Value ? null : Enum.ToObject(type, val);
                            }
                            var evar = System.Convert.ChangeType(val == DBNull.Value ? prop.PropertyType.GetDefaultValue() : val, prop.PropertyType);
                            prop.SetValue(obj, evar);
                        }
                        else
                        {
                            prop.SetValue(obj, prop.PropertyType.GetDefaultValue());
                        }
                    }
                }
                t = obj;
            }
            return t;
        }

        /// <summary>
        /// 获取变量的默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static async Task<IList<T>> ToIListAsync<T>(this IDataReader dr) => await ToListAsync<T>((DbDataReader)dr);

        public static async Task<IList<T>> ToIListAsync<T>(this DbDataReader dr) => await ToListAsync<T>(dr);

        public static async Task<List<T>> ToListAsync<T>(this IDataReader dr) => await ToListAsync<T>((DbDataReader)dr);

        public static async Task<List<T>> ToListAsync<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
                while (await dr.ReadAsync())
                    objList.Add(dr.MapObject<T>(colMapping));
            dr.Close();
            return objList;
        }

        public static List<T> ToList<T>(this IDataReader dr) => ToList<T>((DbDataReader)dr);

        public static IList<T> ToIList<T>(this IDataReader dr) => ToList<T>((DbDataReader)dr);

        public static IList<T> ToIList<T>(this DbDataReader dr) => ToList<T>(dr);

        public static List<T> ToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
                while (dr.Read())
                    objList.Add(dr.MapObject<T>(colMapping));
            dr.Close();
            return objList;
        }

        public static DataTable ToDataTable(this IDataReader dr) => ToDataTable((DbDataReader)dr);

        public static string ToCvsstr(this IDataReader dr) => dr.ToDataTable().ToCsvStr();

        public static DataTable ToDataTable(this DbDataReader dr)
        {
            DataTable objDataTable = new DataTable();
            for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
            {
                objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
            }
            if (dr.HasRows)
            {
                objDataTable.BeginLoadData();
                object[] objValues = new object[dr.FieldCount];
                while (dr.Read())
                {
                    dr.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                objDataTable.EndLoadData();
            }
            dr.Close();
            return objDataTable;
        }

        public static async Task<DataTable> ToDataTableAsync(this IDataReader dr) => await ToDataTableAsync((DbDataReader)dr);

        public static async Task<DataTable> ToDataTableAsync(this DbDataReader dr)
        {
            DataTable objDataTable = new DataTable();
            for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
            {
                objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
            }
            if (dr.HasRows)
            {
                objDataTable.BeginLoadData();
                object[] objValues = new object[dr.FieldCount];
                while (await dr.ReadAsync())
                {
                    dr.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                objDataTable.EndLoadData();
            }
            dr.Close();
            return objDataTable;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IDataReader dr) => await FirstOrDefaultAsync<T>((DbDataReader)dr);

        public static async Task<T> FirstOrDefaultAsync<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            T result = default(T);
            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    result= dr.MapObject<T>(colMapping);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            dr.Close();
            return result;
        }

        public static T FirstOrDefault<T>(this IDataReader dr) => FirstOrDefault<T>((DbDataReader)dr);

        public static T FirstOrDefault<T>(this DbDataReader dr)
        {
            T result = default(T);
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
            {
                while ( dr.Read())
                {
                    result = dr.MapObject<T>(colMapping);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            dr.Close();
            return result;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this IDataReader dr) => await SingleOrDefaultAsync<T>((DbDataReader)dr);

        public static async Task<T> SingleOrDefaultAsync<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (await dr.ReadAsync())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping);
                    hasResult = true;
                }
            dr.Close();
            return obj;
        }

        public static T SingleOrDefault<T>(this IDataReader dr) => SingleOrDefault<T>((DbDataReader)dr);

        public static T SingleOrDefault<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (dr.Read())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping);
                    hasResult = true;
                }
            dr.Close();
            return obj;
        }
    }
}