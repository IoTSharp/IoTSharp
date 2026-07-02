using System;
using System.Text.Json.Nodes;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 提供 System.Text.Json 节点解析入口，统一处理空 JSON 输入。
    /// </summary>
    public static class JsonNodeParser
    {
        /// <summary>
        /// 将 JSON 字符串解析为 JsonNode。
        /// </summary>
        /// <param name="json">需要解析的 JSON 字符串；为空时返回 null。</param>
        /// <returns>解析后的 JsonNode，或 null。</returns>
        public static JsonNode ParseNode(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);
        }

        /// <summary>
        /// 将 JSON 字符串解析为 JsonObject。
        /// </summary>
        /// <param name="json">需要解析的 JSON 字符串；非对象或为空时返回空对象。</param>
        /// <returns>解析后的 JsonObject，或空对象。</returns>
        public static JsonObject ParseObject(string json)
        {
            return ParseNode(json) as JsonObject ?? new JsonObject();
        }

        /// <summary>
        /// 将 JSON 字符串解析为 JsonArray。
        /// </summary>
        /// <param name="json">需要解析的 JSON 字符串；非数组或为空时返回空数组。</param>
        /// <returns>解析后的 JsonArray，或空数组。</returns>
        public static JsonArray ParseArray(string json)
        {
            return ParseNode(json) as JsonArray ?? new JsonArray();
        }
    }
}
