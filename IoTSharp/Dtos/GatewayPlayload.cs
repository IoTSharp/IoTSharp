using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Data
{
    public class GatewayPlayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("ts")]
        public long Ticks { get; set; } = DateTime.UtcNow.Ticks;

        [System.Text.Json.Serialization.JsonPropertyName("values")]
        public Dictionary<string, object> Values { get; set; } = new();
    }
}
