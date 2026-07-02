using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 提供 IoTSharp JSON 工具统一使用的 System.Text.Json 配置。
    /// </summary>
    public static class JsonOptions
    {
        /// <summary>
        /// 获取应用层 JSON 负载默认使用的序列化配置。
        /// </summary>
        public static JsonSerializerOptions Default { get; } = CreateDefault();

        /// <summary>
        /// 创建一份新的 JSON 配置，包含 Web 命名规则、字符串枚举和字符串数字读取支持。
        /// </summary>
        /// <returns>已配置好的 <see cref="JsonSerializerOptions"/> 实例。</returns>
        public static JsonSerializerOptions CreateDefault()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
