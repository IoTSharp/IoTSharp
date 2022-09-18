
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace IoTSharp.Extensions
{
    public static class StringExtension
    {
        #region from  https://github.com/LazyMode/StringEx/blob/master/StringEx.cs


#pragma warning disable 1591

        public static StringComparison GlobalDefaultComparison { get; set; } = StringComparison.Ordinal;

        [ThreadStatic]
        private static StringComparison? _DefaultComparison;
        public static StringComparison DefaultComparison
        {
            get { return _DefaultComparison ?? GlobalDefaultComparison; }
            set { _DefaultComparison = value; }
        }

        #region basic String methods

        public static bool IsNullOrEmpty(this string value)
            => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsWhiteSpace(this string value)
        {
            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c)) continue;

                return false;
            }
            return true;
        }

#if !PCL
        public static string IsInterned(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return string.IsInterned(value);
        }

        public static string Intern(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return string.Intern(value);
        }
#endif

#if UNSAFE
    public static unsafe string ToLowerForASCII(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return value;

        value = string.Copy(value);
        fixed (char* low = value)
        {
            var end = low + value.Length;
            for (var p = low; p < end; p++)
            {
                var c = *p;
                if (c < 'A' || c > 'Z')
                    continue;
                *p = (char)(c + 0x20);
            }
        }
        return value;
    }

    public static unsafe string ToUpperForASCII(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return value;

        value = string.Copy(value);
        fixed (char* low = value)
        {
            var end = low + value.Length;
            for (var p = low; p < end; p++)
            {
                var c = *p;
                if (c < 'a' || c > 'z')
                    continue;
                *p = (char)(c - 0x20);
            }
        }
        return value;
    }
#else
        public static string ToLowerForASCII(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c < 'A' || c > 'Z')
                    sb.Append(c);
                else
                    sb.Append((char)(c + 0x20));
            }
            return sb.ToString();
        }

        public static string ToUpperForASCII(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c < 'a' || c > 'z')
                    sb.Append(c);
                else
                    sb.Append((char)(c - 0x20));
            }
            return sb.ToString();
        }
#endif

        #endregion

        #region comparing

        #region Is

        public static bool Is(this string a, string b)
            => string.Equals(a, b, DefaultComparison);
        public static bool Is(this string a, string b, StringComparison comparisonType)
            => string.Equals(a, b, comparisonType);

        #endregion

        #region BeginWith

        public static bool BeginWith(this string s, char c)
        {
            if (s.IsNullOrEmpty()) return false;
            return s[0] == c;
        }
        public static bool BeginWithAny(this string s, IEnumerable<char> chars)
        {
            if (s.IsNullOrEmpty()) return false;
            return chars.Contains(s[0]);
        }
        public static bool BeginWithAny(this string s, params char[] chars)
            => s.BeginWithAny(chars.AsEnumerable());

        public static bool BeginWith(this string a, string b)
        {
            if (a == null || b == null) return false;

            return a.StartsWith(b, DefaultComparison);
        }
        public static bool BeginWith(this string a, string b, StringComparison comparisonType)
        {
            if (a == null || b == null) return false;

            return a.StartsWith(b, comparisonType);
        }
#if !PCL
        public static bool BeginWith(this string a, string b, bool ignoreCase, CultureInfo culture)
        {
            if (a == null || b == null) return false;

            return a.StartsWith(b, ignoreCase, culture);
        }
#endif

        #endregion

        #region FinishWith

        public static bool FinishWith(this string s, char c)
        {
            if (s.IsNullOrEmpty()) return false;
            return s.Last() == c;
        }
        public static bool FinishWithAny(this string s, IEnumerable<char> chars)
        {
            if (s.IsNullOrEmpty()) return false;
            return chars.Contains(s.Last());
        }
        public static bool FinishWithAny(this string s, params char[] chars)
            => s.FinishWithAny(chars.AsEnumerable());

        public static bool FinishWith(this string a, string b)
        {
            if (a == null || b == null) return false;

            return a.EndsWith(b, DefaultComparison);
        }
        public static bool FinishWith(this string a, string b, StringComparison comparisonType)
        {
            if (a == null || b == null) return false;

            return a.EndsWith(b, comparisonType);
        }
#if !PCL
        public static bool FinishWith(this string a, string b, bool ignoreCase, CultureInfo culture)
        {
            if (a == null || b == null) return false;

            return a.EndsWith(b, ignoreCase, culture);
        }
#endif

        #endregion

        #endregion

        #region ToLines

        public static IEnumerable<string> ToLines(this TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
        public static IEnumerable<string> NonEmptyLines(this TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                yield return line;
            }
        }
        public static IEnumerable<string> NonWhiteSpaceLines(this TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.IsWhiteSpace()) continue;
                yield return line;
            }
        }

        #endregion

        #region others

        private static readonly char[][] Quotes = new[]
        {
        "\"\"",
        "''",
        "“”",
        "‘’",
        "『』",
        "「」",
        "〖〗",
        "【】",
    }.Select(s => s.ToCharArray()).ToArray();
        public static string Enquote(this string value)
        {
            if (value == null)
                return "(null)";

            foreach (var pair in Quotes)
            {
                if (value.IndexOfAny(pair) < 0)
                    return pair[0] + value + pair[1];
            }

            return '"' + value.Replace("\\", @"\\").Replace("\"", @"\""") + '"';
        }

        public static string Replace(this string value, string find, string rep, StringComparison comparsionType)
        {
            if (find.IsNullOrEmpty())
                throw new ArgumentException(null, nameof(find));
            if (rep == null)
                rep = "";
            if (value.IsNullOrEmpty())
                return value;

            var sb = new StringBuilder(value.Length);

            var last = 0;
            var len = find.Length;
            var idx = value.IndexOf(find, DefaultComparison);
            while (idx != -1)
            {
                sb.Append(value.Substring(last, idx - last));
                sb.Append(rep);
                idx += len;

                last = idx;
                idx = value.IndexOf(find, idx, comparsionType);
            }
            sb.Append(value.Substring(last));

            return sb.ToString();
        }
        public static string ReplaceEx(this string value, string find, string rep)
            => value.Replace(find, rep, DefaultComparison);

        #endregion

        #endregion https://github.com/LazyMode/StringEx/blob/master/StringEx.cs


        /// <summary>
        /// 把 \0 也剔除掉。 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string TrimNull(this string buffer)
        {
            return buffer.Trim('\0', '\r', '\n', ' ');
        }

        public static string ToTitleCase(this string str)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
        public static string Left(this string str, int length)
        {
            str = (str ?? string.Empty);
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }

        public static char[] Right(this char[] str, int length)
        {
            return str.Skip(str.Length - length).Take(length).ToArray();
        }
        public static char[] Left(this char[] str, int length)
        {
            return str.Take(length).ToArray();
        }



        public static byte[] Right(this byte[] str, int length)
        {
            return str.Skip(str.Length - length).Take(length).ToArray();
        }
        public static byte[] Left(this byte[] str, int length)
        {
            return str.Take(length).ToArray();
        }
        public static byte[] ToBytes(this string s)
        {
            return System.Text.Encoding.Default.GetBytes(s);
        }
        public static byte[] ToBytes(this string s, Encoding encoding)
        {
            return encoding?.GetBytes(s);
        }


        #region https://github.com/Coldairarrow/EFCore.Sharding/tree/master/src/EFCore.Sharding.Tests/Util



        private static BindingFlags _bindingFlags { get; }
           = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

      

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
        /// 将对象序列化成Json字符串
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj)  where T:class
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 实体类转json数据，速度快
        /// </summary>
        /// <param name="t">实体类</param>
        /// <returns></returns>
        public static string EntityToJson(this object t)
        {
            if (t == null)
                return null;
            string jsonStr = "";
            jsonStr += "{";
            PropertyInfo[] infos = t.GetType().GetProperties();
            for (int i = 0; i < infos.Length; i++)
            {
                jsonStr = jsonStr + "\"" + infos[i].Name + "\":\"" + infos[i].GetValue(t).ToString() + "\"";
                if (i != infos.Length - 1)
                    jsonStr += ",";
            }
            jsonStr += "}";
            return jsonStr;
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            return obj.ToJson().ToObject<T>();
        }

        /// <summary>
        /// 将对象序列化为XML字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string ToXmlStr<T>(this T obj) where T:class
        {
            var jsonStr = obj.ToJson<T>();
            var xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr);
            string xmlDocStr = xmlDoc.InnerXml;

            return xmlDocStr;
        }

        /// <summary>
        /// 将对象序列化为XML字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="rootNodeName">根节点名(建议设为xml)</param>
        /// <returns></returns>
        public static string ToXmlStr<T>(this T obj, string rootNodeName) where T:class
        {
            var jsonStr = obj.ToJson();
            var xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr, rootNodeName);
            string xmlDocStr = xmlDoc.InnerXml;

            return xmlDocStr;
        }

        /// <summary>
        /// 是否拥有某属性
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static bool ContainsProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, _bindingFlags) != null;
        }

        /// <summary>
        /// 获取某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static V GetPropertyValue<T,V>(this T obj, string propertyName) where T:class
        {
            return (V)obj.GetType().GetProperty(propertyName, _bindingFlags).GetValue(obj);
        }

        /// <summary>
        /// 设置某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 是否拥有某字段
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static bool ContainsField(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, _bindingFlags) != null;
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
        /// 改变实体类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static object ChangeType(this object obj, Type targetType)
        {
            return obj.ToJson().ToObject(targetType);
        }

        /// <summary>
        /// 改变实体类型
        /// </summary>
        /// <typeparam name="T">目标泛型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static T ChangeType<T>(this object obj)
        {
            return obj.ToJson().ToObject<T>();
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
        /// string转int
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
                return 0;
            return Convert.ToInt32(str);
        }

        /// <summary>
        /// string转long
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static long ToLong(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToInt64(str);
        }

        /// <summary>
        /// 二进制字符串转为Int
        /// </summary>
        /// <param name="str">二进制字符串</param>
        /// <returns></returns>
        public static int ToInt_FromBinString(this string str)
        {
            return Convert.ToInt32(str, 2);
        }

        /// <summary>
        /// 将16进制字符串转为Int
        /// </summary>
        /// <param name="str">数值</param>
        /// <returns></returns>
        public static int ToInt0X(this string str)
        {
            int num = Int32.Parse(str, NumberStyles.HexNumber);
            return num;
        }

        /// <summary>
        /// 转换为double
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }



        /// <summary>
        /// 将16进制字符串转为Byte数组
        /// </summary>
        /// <param name="str">16进制字符串(2个16进制字符表示一个Byte)</param>
        /// <returns></returns>
        public static byte[] To0XBytes(this string str)
        {
            List<byte> resBytes = new List<byte>();
            for (int i = 0; i < str.Length; i = i + 2)
            {
                string numStr = $@"{str[i]}{str[i + 1]}";
                resBytes.Add((byte)numStr.ToInt0X());
            }

            return resBytes.ToArray();
        }

        /// <summary>
        /// 将ASCII码形式的字符串转为对应字节数组
        /// 注：一个字节一个ASCII码字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static byte[] ToASCIIBytes(this string str)
        {
            return str.ToList().Select(x => (byte)x).ToArray();
        }

        /// <summary>
        /// 转换为日期格式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str)
        {
            return Convert.ToDateTime(str);
        }

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
        /// 删除Json字符串中键中的@符号
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        /// <returns></returns>
        public static string RemoveAt(this string jsonStr)
        {
            Regex reg = new Regex("\"@([^ \"]*)\"\\s*:\\s*\"(([^ \"]+\\s*)*)\"");
            string strPatten = "\"$1\":\"$2\"";
            return reg.Replace(jsonStr, strPatten);
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
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static T XmlStrToObject<T>(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<T>(jsonJsonStr);
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static JObject XmlStrToJObject(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<JObject>(jsonJsonStr);
        }

        /// <summary>
        /// 将Json字符串转为List'T'
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this string jsonStr)
        {
            return string.IsNullOrEmpty(jsonStr) ? null : JsonConvert.DeserializeObject<List<T>>(jsonStr);
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

        /// <summary>
        /// 将Json字符串转为JObject
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static JObject ToJObject(this string jsonStr)
        {
            return jsonStr == null ? JObject.Parse("{}") : JObject.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 将Json字符串转为JArray
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static JArray ToJArray(this string jsonStr)
        {
            return jsonStr == null ? JArray.Parse("[]") : JArray.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// json数据转实体类,仅仅应用于单个实体类，速度非常快
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T ToEntity<T>(this string json)
        {
            if (json == null || json == "")
                return default(T);

            Type type = typeof(T);
            object obj = Activator.CreateInstance(type, null);

            foreach (var item in type.GetProperties())
            {
                PropertyInfo info = obj.GetType().GetProperty(item.Name);
                string pattern;
                pattern = "\"" + item.Name + "\":\"(.*?)\"";
                foreach (Match match in Regex.Matches(json, pattern))
                {
                    switch (item.PropertyType.ToString())
                    {
                        case "System.String": info.SetValue(obj, match.Groups[1].ToString(), null); break;
                        case "System.Int32": info.SetValue(obj, match.Groups[1].ToString().ToInt(), null); ; break;
                        case "System.Int64": info.SetValue(obj, Convert.ToInt64(match.Groups[1].ToString()), null); ; break;
                        case "System.DateTime": info.SetValue(obj, Convert.ToDateTime(match.Groups[1].ToString()), null); ; break;
                    }
                }
            }
            return (T)obj;
        }

        /// <summary>
        /// 转为首字母大写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToFirstUpperStr(this string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// 转为首字母小写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToFirstLowerStr(this string str)
        {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        /// <summary>
        /// 转为网络终结点IPEndPoint
        /// </summary>=
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static IPEndPoint ToIPEndPoint(this string str)
        {
            IPEndPoint iPEndPoint = null;
            try
            {
                string[] strArray = str.Split(':').ToArray();
                string addr = strArray[0];
                int port = Convert.ToInt32(strArray[1]);
                iPEndPoint = new IPEndPoint(IPAddress.Parse(addr), port);
            }
            catch
            {
                iPEndPoint = null;
            }

            return iPEndPoint;
        }

        /// <summary>
        /// 是否为弱密码
        /// 注:密码必须包含数字、小写字母、大写字母和其他符号中的两种并且长度大于8
        /// </summary>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public static bool IsWeakPwd(this string pwd)
        {
            if (pwd.IsNullOrEmpty())
                throw new Exception("pwd不能为空");

            string pattern = "(^[0-9]+$)|(^[a-z]+$)|(^[A-Z]+$)|(^.{0,8}$)";
            if (Regex.IsMatch(pwd, pattern))
                return true;
            else
                return false;
        }
        #endregion
    }
}
