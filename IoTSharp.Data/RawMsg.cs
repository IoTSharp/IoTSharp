using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Playload
    {
        [JsonProperty(PropertyName = "ts")]
        public long Ticks { get; set; } = DateTime.Now.Ticks;
        [JsonProperty(PropertyName = "devicestatus")]
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Good;
        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, object> Values { get; set; } = new();
    }
    public enum DeviceStatus
    {
        Good,
        PartGood,
        Bad,
        UnKnow
    }
    
    public class RawMsg
    {
        public MsgType MsgType { get; set; }
        public DateTime ts { get; set; } = DateTime.Now;
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Good;
        public Guid DeviceId { get;   set; }
        public Dictionary<string, object> MsgBody { get;  set; }
        public DataSide DataSide { get;  set; }
        public DataCatalog DataCatalog { get;  set; }
    }
}
