using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;

namespace IoTSharp.Extensions
{
    public static class XmlJsonConverter
    {
        public static string SerializeXmlNode(XmlNode node)
        {
            if (node is XmlDocument document)
            {
                node = document.DocumentElement;
            }

            var root = new JsonObject();
            if (node != null)
            {
                root[node.Name] = ConvertXmlNode(node);
            }

            return root.ToJsonString(JsonOptions.Default);
        }

        public static XmlDocument DeserializeXmlNode(string json, string rootNodeName = null)
        {
            var document = new XmlDocument();
            var node = JsonNodeParser.ParseNode(json);
            var rootName = rootNodeName;

            if (string.IsNullOrWhiteSpace(rootName)
                && node is JsonObject obj
                && obj.Count == 1)
            {
                var property = obj.First();
                rootName = property.Key;
                node = property.Value;
            }

            rootName = string.IsNullOrWhiteSpace(rootName) ? "root" : rootName;
            var root = document.CreateElement(rootName);
            document.AppendChild(root);
            AppendJsonNode(document, root, node);
            return document;
        }

        private static JsonNode ConvertXmlNode(XmlNode node)
        {
            var element = node as XmlElement;
            if (element == null)
            {
                return JsonValue.Create(node.InnerText);
            }

            var value = new JsonObject();
            if (element.HasAttributes)
            {
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    value[$"@{attribute.Name}"] = attribute.Value;
                }
            }

            var childElements = element.ChildNodes.OfType<XmlElement>().ToList();
            foreach (var group in childElements.GroupBy(child => child.Name))
            {
                if (group.Count() == 1)
                {
                    value[group.Key] = ConvertXmlNode(group.First());
                    continue;
                }

                var array = new JsonArray();
                foreach (var child in group)
                {
                    array.Add(ConvertXmlNode(child));
                }

                value[group.Key] = array;
            }

            var text = string.Concat(element.ChildNodes
                .OfType<XmlCharacterData>()
                .Select(child => child.Value));

            if (!string.IsNullOrWhiteSpace(text))
            {
                if (value.Count == 0)
                {
                    return JsonValue.Create(text);
                }

                value["#text"] = text;
            }

            return value;
        }

        private static void AppendJsonNode(XmlDocument document, XmlElement element, JsonNode node)
        {
            switch (node)
            {
                case null:
                    return;
                case JsonObject obj:
                    foreach (var property in obj)
                    {
                        if (property.Key.StartsWith("@", StringComparison.Ordinal))
                        {
                            element.SetAttribute(property.Key[1..], property.Value?.GetStringValue() ?? string.Empty);
                            continue;
                        }

                        if (property.Key == "#text")
                        {
                            element.AppendChild(document.CreateTextNode(property.Value?.GetStringValue() ?? string.Empty));
                            continue;
                        }

                        if (property.Value is JsonArray array)
                        {
                            foreach (var item in array)
                            {
                                var child = document.CreateElement(property.Key);
                                element.AppendChild(child);
                                AppendJsonNode(document, child, item);
                            }
                        }
                        else
                        {
                            var child = document.CreateElement(property.Key);
                            element.AppendChild(child);
                            AppendJsonNode(document, child, property.Value);
                        }
                    }
                    break;
                case JsonArray array:
                    foreach (var item in array)
                    {
                        var child = document.CreateElement("item");
                        element.AppendChild(child);
                        AppendJsonNode(document, child, item);
                    }
                    break;
                default:
                    element.InnerText = node.GetStringValue() ?? string.Empty;
                    break;
            }
        }
    }
}
