
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

        private static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

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

        private static JsonNode ConvertElementValue(System.Xml.Linq.XElement element)
        {
            return element.HasElements || element.HasAttributes
                ? ConvertElementToJsonObject(element)
                : JsonValue.Create(element.Value);
        }
    }
}
