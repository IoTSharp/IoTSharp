using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 在 XML DOM 与 System.Text.Json DOM 之间转换，替代旧的 XML JSON 转换能力。
    /// </summary>
    public static class XmlJsonConverter
    {
        /// <summary>
        /// 将 XML 节点序列化为 JSON 字符串，根元素名称会作为 JSON 根属性。
        /// </summary>
        /// <param name="node">需要转换的 XML 节点；传入 XmlDocument 时使用其 DocumentElement。</param>
        /// <returns>表示 XML 内容的 JSON 字符串。</returns>
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

        /// <summary>
        /// 将 JSON 字符串还原为 XML 文档。
        /// </summary>
        /// <param name="json">需要转换的 JSON 字符串。</param>
        /// <param name="rootNodeName">可选根节点名称；为空时优先使用 JSON 单根属性名。</param>
        /// <returns>转换后的 XML 文档。</returns>
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

        /// <summary>
        /// 将单个 XML 节点转换为 JsonNode，属性使用 @ 前缀，文本使用 #text。
        /// </summary>
        /// <param name="node">需要转换的 XML 节点。</param>
        /// <returns>表示该 XML 节点的 JsonNode。</returns>
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

        /// <summary>
        /// 将 JsonNode 内容追加到 XML 元素中。
        /// </summary>
        /// <param name="document">用于创建节点的 XML 文档。</param>
        /// <param name="element">承载追加内容的 XML 元素。</param>
        /// <param name="node">需要追加的 JSON 节点。</param>
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
