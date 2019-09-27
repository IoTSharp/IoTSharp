using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                string jsonText = JsonConvert.SerializeXmlNode(doc.DocumentElement);
                obj = JsonConvert.DeserializeObject(jsonText);
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
                string jsonText = JsonConvert.SerializeXmlNode(doc.DocumentElement);
                obj = JsonConvert.DeserializeObject(jsonText);
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
                string jsonText = JsonConvert.SerializeXmlNode(doc.DocumentElement);
                obj = JsonConvert.DeserializeObject(jsonText);
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
                    var emls = from exx in em.Elements().FirstOrDefault(xe => xe.Name.LocalName == typeof(T).Name).Elements() select JObject.FromObject(exx).GetValue("Table").ToObject<T>();
                    ts.AddRange(emls.ToArray());
                });
            }
            catch (Exception)
            {

            }
            return ts;
        }
    }
}
