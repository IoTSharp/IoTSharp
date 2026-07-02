
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;

namespace IoTSharp.Extensions
{
    public static class XmlExtensions
    {

        /// <summary>
        /// 将内存流中的 XML 转换为普通 CLR 对象形式的 JSON 结构。
        /// </summary>
        /// <param name="xml">包含 XML 内容的内存流。</param>
        /// <param name="obj">转换后的对象；失败时为 null。</param>
        /// <returns>转换成功返回 true，否则返回 false。</returns>
        public static bool XML2Json(System.IO.MemoryStream xml, out object obj)
        {
            bool ok = false;
            obj = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                xml.Seek(0, System.IO.SeekOrigin.Begin);
                doc.Load(xml);
                string jsonText = XmlJsonConverter.SerializeXmlNode(doc.DocumentElement);
                obj = JsonObjectSerializer.DeserializeUntyped(jsonText);
                ok = true;
            }
            catch (Exception)
            {
            }
            return ok;
        }

        /// <summary>
        /// 将 XML 文件转换为普通 CLR 对象形式的 JSON 结构。
        /// </summary>
        /// <param name="xml">XML 文件信息。</param>
        /// <param name="obj">转换后的对象；失败时为 null。</param>
        /// <returns>转换成功返回 true，否则返回 false。</returns>
        public static bool XML2Json(FileInfo xml, out object obj)
        {
            bool ok = false;
            obj = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml.FullName);
                string jsonText = XmlJsonConverter.SerializeXmlNode(doc.DocumentElement);
                obj = JsonObjectSerializer.DeserializeUntyped(jsonText);
                ok = true;
            }
            catch (Exception)
            {
            }
            return ok;
        }

        /// <summary>
        /// 将 XML 字符串转换为普通 CLR 对象形式的 JSON 结构。
        /// </summary>
        /// <param name="xml">XML 字符串；包含非法字符时会先过滤。</param>
        /// <param name="obj">转换后的对象；失败时为 null。</param>
        /// <returns>转换成功返回 true，否则返回 false。</returns>
        public static bool XML2Json(string xml, out object obj)
        {
            bool ok = false;
            obj = null;
            try
            {
                if (!IsValidXmlString(xml))
                {
                    xml = RemoveInvalidXmlChars(xml);
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                string jsonText = XmlJsonConverter.SerializeXmlNode(doc.DocumentElement);
                obj = JsonObjectSerializer.DeserializeUntyped(jsonText);
                ok = true;
            }
            catch (Exception)
            {
            }
            return ok;
        }

        /// <summary>
        /// 过滤 XML 规范不允许的字符。
        /// </summary>
        /// <param name="text">需要清理的文本。</param>
        /// <returns>只包含合法 XML 字符的文本。</returns>
        private static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        /// <summary>
        /// 判断字符串是否只包含 XML 规范允许的字符。
        /// </summary>
        /// <param name="text">需要检查的文本。</param>
        /// <returns>全部字符合法返回 true，否则返回 false。</returns>
        private static bool IsValidXmlString(string text)
        {
            try
            {
                XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将 diffgram 形式的 XElement 列表转换为指定实体列表。
        /// </summary>
        /// <typeparam name="T">目标实体类型。</typeparam>
        /// <param name="_Nodes">包含 diffgram 节点的 XElement 列表。</param>
        /// <returns>转换后的实体列表；无法解析的节点会被忽略。</returns>
        public static List<T> ToList<T>(this List<System.Xml.Linq.XElement> _Nodes)
        {
            if (_Nodes == null)
            {
                throw new ArgumentNullException(nameof(_Nodes));
            }
            List<T> ts = new List<T>();
            try
            {
                var ems = from ex in _Nodes where ex.Name.LocalName == "diffgram" select ex;
                ems.ToList().ForEach(em =>
                {
                    var parent = em.Elements().FirstOrDefault(xe => xe.Name.LocalName == typeof(T).Name);
                    if (parent == null)
                    {
                        return;
                    }

                    var emls = from exx in parent.Elements()
                               let json = ConvertElementToJsonObject(exx)
                               select json.ToObject<T>();
                    ts.AddRange(emls.ToArray());
                });
            }
            catch (Exception)
            {

            }
            return ts;
        }

        /// <summary>
        /// 将 XElement 的属性和子元素转换为 JsonObject，供后续类型反序列化使用。
        /// </summary>
        /// <param name="element">需要转换的 XML 元素。</param>
        /// <returns>表示该元素内容的 JsonObject。</returns>
        private static JsonObject ConvertElementToJsonObject(System.Xml.Linq.XElement element)
        {
            var result = new JsonObject();
            foreach (var attribute in element.Attributes())
            {
                result[attribute.Name.LocalName] = attribute.Value;
            }

            var childGroups = element.Elements().GroupBy(child => child.Name.LocalName);
            foreach (var group in childGroups)
            {
                if (group.Count() == 1)
                {
                    result[group.Key] = ConvertElementValue(group.First());
                    continue;
                }

                var array = new JsonArray();
                foreach (var child in group)
                {
                    array.Add(ConvertElementValue(child));
                }

                result[group.Key] = array;
            }

            if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            {
                result[element.Name.LocalName] = element.Value;
            }

            return result;
        }

        /// <summary>
        /// 根据 XElement 是否包含子节点或属性，转换为对象节点或字符串节点。
        /// </summary>
        /// <param name="element">需要转换的 XML 元素。</param>
        /// <returns>转换后的 JsonNode。</returns>
        private static JsonNode ConvertElementValue(System.Xml.Linq.XElement element)
        {
            return element.HasElements || element.HasAttributes
                ? ConvertElementToJsonObject(element)
                : JsonValue.Create(element.Value);
        }
    }
}
