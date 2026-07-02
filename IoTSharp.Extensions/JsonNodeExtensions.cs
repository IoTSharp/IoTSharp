using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 为 System.Text.Json 节点提供路径选择和 CLR 值转换能力。
    /// </summary>
    public static class JsonNodeExtensions
    {
        /// <summary>
        /// 按简单 JSON 路径选择子节点，例如 $.device.name 或 values[0]。
        /// </summary>
        /// <param name="node">用于查找的根节点。</param>
        /// <param name="path">点号或方括号路径；为空时返回原节点。</param>
        /// <returns>选中的节点；路径无法解析时返回 null。</returns>
        public static JsonNode SelectByPath(this JsonNode node, string path)
        {
            if (node == null || string.IsNullOrWhiteSpace(path))
            {
                return node;
            }

            var current = node;
            foreach (var segment in ParsePath(path))
            {
                if (current == null)
                {
                    return null;
                }

                current = segment.Match(
                    name => current as JsonObject is { } obj && obj.TryGetPropertyValue(name, out var child) ? child : null,
                    index => current as JsonArray is { } array && index >= 0 && index < array.Count ? array[index] : null);
            }

            return current;
        }

        /// <summary>
        /// 将 JsonNode 转换为 ExpandoObject、列表、标量值或 null。
        /// </summary>
        /// <param name="node">需要转换的 JSON 节点。</param>
        /// <returns>表示该 JSON 节点的 CLR 对象。</returns>
        public static object ToClrObject(this JsonNode node)
        {
            if (node == null)
            {
                return null;
            }

            using var document = JsonDocument.Parse(node.ToJsonString(JsonOptions.Default));
            return document.RootElement.ToClrObject();
        }

        /// <summary>
        /// 将 JsonNode 反序列化为指定 CLR 类型。
        /// </summary>
        /// <typeparam name="T">目标 CLR 类型。</typeparam>
        /// <param name="node">需要反序列化的 JSON 节点。</param>
        /// <returns>反序列化后的对象；节点为 null 时返回默认值。</returns>
        public static T ToObject<T>(this JsonNode node)
        {
            return node == null ? default : node.Deserialize<T>(JsonOptions.Default);
        }

        /// <summary>
        /// 将 JsonNode 反序列化为运行时指定的 CLR 类型。
        /// </summary>
        /// <param name="node">需要反序列化的 JSON 节点。</param>
        /// <param name="type">目标 CLR 类型。</param>
        /// <returns>反序列化后的对象；节点为 null 时返回 null。</returns>
        public static object ToObject(this JsonNode node, Type type)
        {
            return node == null ? null : node.Deserialize(type, JsonOptions.Default);
        }

        /// <summary>
        /// 将 JSON 对象或对象数组展开为键值字典。
        /// </summary>
        /// <param name="node">需要转换的 JSON 节点。</param>
        /// <returns>值已转换为 CLR 对象的字典。</returns>
        public static Dictionary<string, object> ToDictionary(this JsonNode node)
        {
            var result = new Dictionary<string, object>();
            if (node is JsonArray array)
            {
                foreach (var item in array.OfType<JsonObject>())
                {
                    foreach (var property in item)
                    {
                        result[property.Key] = property.Value.ToClrObject();
                    }
                }

                return result;
            }

            if (node is not JsonObject obj)
            {
                return result;
            }

            foreach (var property in obj)
            {
                result[property.Key] = property.Value.ToClrObject();
            }

            return result;
        }

        /// <summary>
        /// 将标量节点读取为字符串；复杂节点返回紧凑 JSON。
        /// </summary>
        /// <param name="node">需要读取的节点。</param>
        /// <returns>字符串值、JSON 字符串或 null。</returns>
        public static string GetStringValue(this JsonNode node)
        {
            return node switch
            {
                null => null,
                JsonValue value when value.TryGetValue<string>(out var text) => text,
                _ => node.ToJsonString(JsonOptions.Default)
            };
        }

        /// <summary>
        /// 将数字节点读取为 double，并兼容字符串形式的数字。
        /// </summary>
        /// <param name="node">需要读取的节点。</param>
        /// <returns>解析后的 double；解析失败时返回 0。</returns>
        public static double GetDoubleValue(this JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<double>(out var doubleValue))
                {
                    return doubleValue;
                }

                if (value.TryGetValue<string>(out var text)
                    && double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
                {
                    return doubleValue;
                }
            }

            return 0d;
        }

        /// <summary>
        /// 将支持的属性路径和数组路径解析为导航片段。
        /// </summary>
        /// <param name="path">需要解析的路径文本。</param>
        /// <returns>按顺序排列的路径片段。</returns>
        private static IEnumerable<PathSegment> ParsePath(string path)
        {
            var normalized = path.Trim();
            if (normalized.StartsWith("$.", StringComparison.Ordinal))
            {
                normalized = normalized[2..];
            }
            else if (normalized == "$")
            {
                yield break;
            }
            else if (normalized.StartsWith("$", StringComparison.Ordinal))
            {
                normalized = normalized[1..];
            }

            var name = string.Empty;
            for (var i = 0; i < normalized.Length; i++)
            {
                var ch = normalized[i];
                if (ch == '.')
                {
                    if (name.Length > 0)
                    {
                        yield return PathSegment.Name(name);
                        name = string.Empty;
                    }

                    continue;
                }

                if (ch == '[')
                {
                    if (name.Length > 0)
                    {
                        yield return PathSegment.Name(name);
                        name = string.Empty;
                    }

                    var end = normalized.IndexOf(']', i + 1);
                    if (end < 0)
                    {
                        yield break;
                    }

                    var token = normalized[(i + 1)..end].Trim('\'', '"', ' ');
                    if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                    {
                        yield return PathSegment.Index(index);
                    }
                    else if (!string.IsNullOrEmpty(token))
                    {
                        yield return PathSegment.Name(token);
                    }

                    i = end;
                    continue;
                }

                name += ch;
            }

            if (name.Length > 0)
            {
                yield return PathSegment.Name(name);
            }
        }

        private readonly struct PathSegment
        {
            private readonly string _name;
            private readonly int _index;
            private readonly bool _isIndex;

            private PathSegment(string name, int index, bool isIndex)
            {
                _name = name;
                _index = index;
                _isIndex = isIndex;
            }

            public static PathSegment Name(string name) => new(name, -1, false);

            public static PathSegment Index(int index) => new(null, index, true);

            public T Match<T>(Func<string, T> name, Func<int, T> index)
            {
                return _isIndex ? index(_index) : name(_name);
            }
        }
    }
}
