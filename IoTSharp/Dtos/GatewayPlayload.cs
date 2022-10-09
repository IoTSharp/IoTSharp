using IoTSharp.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IoTSharp.Data
{
    public class GatewayPlayload
    {
        [JsonProperty(PropertyName = "ts")]
        public long Ticks { get; set; } = DateTime.Now.Ticks;

        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, object> Values { get; set; } = new();
    }
}
