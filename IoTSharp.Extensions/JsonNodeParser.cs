using System;
using System.Text.Json.Nodes;

namespace IoTSharp.Extensions
{
    public static class JsonNodeParser
    {
        public static JsonNode ParseNode(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);
        }

        public static JsonObject ParseObject(string json)
        {
            return ParseNode(json) as JsonObject ?? new JsonObject();
        }

        public static JsonArray ParseArray(string json)
        {
            return ParseNode(json) as JsonArray ?? new JsonArray();
        }
    }
}
