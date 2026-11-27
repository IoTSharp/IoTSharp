using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace IoTSharp.Data.Extensions
{
    public static class PlayloadExtension
    {
        /// <summary>按原字段名和插入顺序生成规则使用的动态对象。</summary>
        public static ExpandoObject ToDynamic(this Dictionary<string, object> dc)
        {
            ArgumentNullException.ThrowIfNull(dc, "source");
            ExpandoObject obj = new();
            foreach (var kv in dc)
            {
                obj.TryAdd(kv.Key, kv.Value);
            }
            return obj;
        }

        /// <summary>保留每个字段首次出现的值，供规则按名称访问。</summary>
        public static ExpandoObject ToDynamic(this List<TelemetryDataDto> array)
        {
            ExpandoObject exps = new();
            foreach (var td in array)
            {
                exps.TryAdd(td.KeyName, td.Value);
            }
            return exps;
        }

        /// <summary>按消息字段顺序转换 JSON 值，忽略空值并保留对象和数组的原始 JSON。</summary>
        public static Dictionary<string, object> ToDictionary(this PlayloadData msg)
        {
            var mb = msg.MsgBody;
            ArgumentNullException.ThrowIfNull(mb, "source");
            Dictionary<string, object> dc = new();
            foreach (var kp in mb)
            {
                if (kp.Value is JsonElement je)
                {
                    switch (je.ValueKind)
                    {
                        case JsonValueKind.Undefined:
                        case JsonValueKind.Object:
                        case JsonValueKind.Array:
                            dc.Add(kp.Key, je.GetRawText());
                            break;

                        case JsonValueKind.String:
                            dc.Add(kp.Key, je.GetString());
                            break;

                        case JsonValueKind.Number:
                            dc.Add(kp.Key, je.GetDouble());
                            break;

                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            dc.Add(kp.Key, je.GetBoolean());
                            break;

                        case JsonValueKind.Null:
                            break;

                        default:
                            break;
                    }
                }
                else if (kp.Value != null)
                {
                    dc.Add(kp.Key, kp.Value);
                }
            }
            return dc;
        }
    }
}
