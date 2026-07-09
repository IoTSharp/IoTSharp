using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// CoAP 接入层使用的 System.Text.Json 源生成上下文，避免热路径依赖反射元数据。
    /// </summary>
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(CoapAlarmPayload))]
    internal sealed partial class CoapJsonSerializerContext : JsonSerializerContext
    {
        /// <summary>
        /// CoAP 接入层共享的源生成上下文。
        /// </summary>
        public static CoapJsonSerializerContext Shared { get; } = new(CreateOptions());

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true
            };
            return options;
        }
    }
}
