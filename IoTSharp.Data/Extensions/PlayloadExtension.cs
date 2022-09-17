using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data.Extensions
{
    public static class PlayloadExtension
    {
        public static ExpandoObject ToDynamic( this Dictionary<string, object> dc)
        {
            ExpandoObject obj = new ExpandoObject();
            dc.ToList().ForEach(kv =>
            {
                obj.TryAdd(kv.Key, kv.Value);
            });
            return obj;
        }
        public static ExpandoObject ToDynamic(this List <TelemetryDataDto> array)
        {
            ExpandoObject exps = new();
            array.ForEach(td =>
            {
                exps.TryAdd(td.KeyName, td.Value);
            });
            return exps;
        }
        public static Dictionary<string, object> ToDictionary(this PlayloadData msg)
        {
            var mb = msg.MsgBody;
            Dictionary<string, object> dc = new Dictionary<string, object>();
            mb.ToList().ForEach(kp =>
            {
                if (kp.Value?.GetType() == typeof(System.Text.Json.JsonElement))
                {
                    var je = (System.Text.Json.JsonElement)kp.Value;
                    switch (je.ValueKind)
                    {
                        case System.Text.Json.JsonValueKind.Undefined:
                        case System.Text.Json.JsonValueKind.Object:
                        case System.Text.Json.JsonValueKind.Array:
                            dc.Add(kp.Key, je.GetRawText());
                            break;

                        case System.Text.Json.JsonValueKind.String:
                            dc.Add(kp.Key, je.GetString());
                            break;

                        case System.Text.Json.JsonValueKind.Number:
                            dc.Add(kp.Key, je.GetDouble());
                            break;

                        case System.Text.Json.JsonValueKind.True:
                        case System.Text.Json.JsonValueKind.False:
                            dc.Add(kp.Key, je.GetBoolean());
                            break;

                        case System.Text.Json.JsonValueKind.Null:
                            break;

                        default:
                            break;
                    }
                }
                else if (kp.Value != null)
                {
                    dc.Add(kp.Key, kp.Value);
                }

            });
            return dc;
        }
    }
}
