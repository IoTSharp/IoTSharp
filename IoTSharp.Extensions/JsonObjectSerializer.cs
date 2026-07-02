using System;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 集中封装 JSON 序列化入口，确保调用方使用同一套 System.Text.Json 配置。
    /// </summary>
    public static class JsonObjectSerializer
    {
        /// <summary>
        /// 使用统一 JSON 配置序列化对象。
        /// </summary>
        /// <param name="value">需要序列化的对象。</param>
        /// <returns><paramref name="value"/> 对应的 JSON 字符串。</returns>
        public static string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, JsonOptions.Default);
        }

        /// <summary>
        /// 将对象序列化为可变的 System.Text.Json DOM 节点。
        /// </summary>
        /// <param name="value">需要序列化的对象；为 null 时返回 null。</param>
        /// <returns>表示该对象的 <see cref="JsonNode"/>，或 null。</returns>
        public static JsonNode SerializeToNode(object value)
        {
            return value == null ? null : JsonSerializer.SerializeToNode(value, JsonOptions.Default);
        }

        /// <summary>
        /// 使用统一 JSON 配置反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 CLR 类型。</typeparam>
        /// <param name="json">需要读取的 JSON 字符串；为空时返回默认值。</param>
        /// <returns>反序列化后的对象，或默认值。</returns>
        public static T Deserialize<T>(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions.Default);
        }

        /// <summary>
        /// 反序列化为运行时指定的 CLR 类型。
        /// </summary>
        /// <param name="json">需要读取的 JSON 字符串；为空时返回 null。</param>
        /// <param name="type">目标 CLR 类型。</param>
        /// <returns>反序列化后的对象，或 null。</returns>
        public static object Deserialize(string json, Type type)
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize(json, type, JsonOptions.Default);
        }

        /// <summary>
        /// 将 JSON 读取为普通 CLR 值，避免把动态值保留为 JsonElement。
        /// </summary>
        /// <param name="json">需要读取的 JSON 字符串；为空时返回 null。</param>
        /// <returns>与 JSON 结构匹配的 ExpandoObject、列表、标量值或 null。</returns>
        public static object DeserializeUntyped(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using var document = JsonDocument.Parse(json);
            return document.RootElement.ToClrObject();
        }

        /// <summary>
        /// 将 JSON 对象读取为 ExpandoObject，供脚本引擎和动态规则输出使用。
        /// </summary>
        /// <param name="json">需要读取的 JSON 对象字符串；空内容或非对象内容返回空 ExpandoObject。</param>
        /// <returns>包含 JSON 对象属性的 ExpandoObject。</returns>
        public static ExpandoObject DeserializeExpando(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ExpandoObject();
            }

            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object
                ? document.RootElement.ToExpandoObject()
                : new ExpandoObject();
        }

        /// <summary>
        /// 将 JSON 对象数组转换为 DataTable，列类型统一为 object。
        /// </summary>
        /// <param name="json">需要读取的 JSON 数组字符串；为空时返回 null。</param>
        /// <returns>由数组行填充的 DataTable。</returns>
        public static DataTable DeserializeDataTable(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var table = new DataTable();
            var rows = JsonNodeParser.ParseNode(json) as JsonArray;
            if (rows == null)
            {
                return table;
            }

            foreach (var row in rows.OfType<JsonObject>())
            {
                foreach (var property in row)
                {
                    if (!table.Columns.Contains(property.Key))
                    {
                        table.Columns.Add(property.Key, typeof(object));
                    }
                }

                var dataRow = table.NewRow();
                foreach (var property in row)
                {
                    dataRow[property.Key] = property.Value.ToClrObject() ?? DBNull.Value;
                }

                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}
