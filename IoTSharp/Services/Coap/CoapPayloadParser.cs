using IoTSharp.Contracts;
using IoTSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// CoAP payload 的 UTF-8 解析工具，保持协议层的 <see cref="System.ReadOnlyMemory{T}"/> 所有权语义。
    /// </summary>
    internal static class CoapPayloadParser
    {
        private static readonly JsonDocumentOptions DocumentOptions = new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow
        };

        /// <summary>
        /// 将遥测或属性 payload 读取为业务事件总线使用的字典。
        /// </summary>
        /// <param name="payload">CoAP 请求中的 UTF-8 JSON payload。</param>
        /// <returns>属性值已转换为 CLR 对象的字典。</returns>
        /// <exception cref="JsonException">payload 不是 JSON 对象或对象数组时抛出。</exception>
        public static Dictionary<string, object> ParseObject(ReadOnlyMemory<byte> payload)
        {
            using var document = JsonDocument.Parse(payload, DocumentOptions);
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                AddObjectProperties(root, result);
                return result;
            }

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        AddObjectProperties(item, result);
                    }
                }

                return result;
            }

            throw new JsonException("CoAP payload must be a JSON object.");
        }

        /// <summary>
        /// 使用源生成元数据读取告警 payload。
        /// </summary>
        /// <param name="payload">CoAP 请求中的 UTF-8 JSON payload。</param>
        /// <returns>解析得到的告警 DTO。</returns>
        public static CreateAlarmDto ParseAlarm(ReadOnlyMemory<byte> payload)
        {
            var alarmPayload = JsonSerializer.Deserialize(
                payload.Span,
                CoapJsonSerializerContext.Shared.CoapAlarmPayload);
            return alarmPayload?.ToCreateAlarmDto();
        }

        private static void AddObjectProperties(
            JsonElement element,
            Dictionary<string, object> destination)
        {
            foreach (var property in element.EnumerateObject())
            {
                destination[property.Name] = property.Value.ToClrObject();
            }
        }
    }

    /// <summary>
    /// CoAP 告警入口的专用反序列化模型，避免热路径受到契约枚举上的反射型 converter 影响。
    /// </summary>
    internal sealed class CoapAlarmPayload
    {
        /// <summary>
        /// 起因对象名称；为空时业务层会回填当前设备名称。
        /// </summary>
        public string OriginatorName { get; set; }

        /// <summary>
        /// 告警类型。
        /// </summary>
        public string AlarmType { get; set; }

        /// <summary>
        /// 告警详情。
        /// </summary>
        public string AlarmDetail { get; set; }

        /// <summary>
        /// 严重程度，兼容字符串和整数枚举值。
        /// </summary>
        public JsonElement? Serverity { get; set; }

        /// <summary>
        /// 关联的告警数据 ID。
        /// </summary>
        public Guid WarnDataId { get; set; }

        /// <summary>
        /// 告警创建时间。
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// 起因对象类型，兼容字符串和整数枚举值。
        /// </summary>
        public JsonElement? OriginatorType { get; set; }

        /// <summary>
        /// 转换为现有事件总线使用的告警 DTO。
        /// </summary>
        /// <returns>平台告警 DTO。</returns>
        public CreateAlarmDto ToCreateAlarmDto()
        {
            return new CreateAlarmDto
            {
                OriginatorName = OriginatorName,
                AlarmType = AlarmType,
                AlarmDetail = AlarmDetail,
                Serverity = ReadEnum(Serverity, ServerityLevel.Indeterminate),
                warnDataId = WarnDataId,
                CreateDateTime = CreateDateTime,
                OriginatorType = ReadEnum(OriginatorType, IoTSharp.Contracts.OriginatorType.Unknow)
            };
        }

        private static TEnum ReadEnum<TEnum>(JsonElement? value, TEnum defaultValue)
            where TEnum : struct, Enum
        {
            if (!value.HasValue
                || value.Value.ValueKind == JsonValueKind.Null
                || value.Value.ValueKind == JsonValueKind.Undefined)
            {
                return defaultValue;
            }

            if (value.Value.ValueKind == JsonValueKind.String)
            {
                var text = value.Value.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return defaultValue;
                }

                if (Enum.TryParse<TEnum>(text, ignoreCase: true, out var parsed))
                {
                    return parsed;
                }

                throw new JsonException($"Invalid {typeof(TEnum).Name} value.");
            }

            if (value.Value.ValueKind == JsonValueKind.Number
                && value.Value.TryGetInt32(out var number))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), number);
            }

            throw new JsonException($"Invalid {typeof(TEnum).Name} value.");
        }
    }
}
