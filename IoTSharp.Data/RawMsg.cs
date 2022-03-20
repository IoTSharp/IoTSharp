using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Playload
    {
        public long Ts { get; set; } = DateTime.Now.Ticks;
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Good;
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
        public long ts { get; set; } = DateTime.Now.Ticks;
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Good;
        public Guid DeviceId { get;   set; }
        public Dictionary<string, object> MsgBody { get;  set; }
        public DataSide DataSide { get;  set; }
        public DataCatalog DataCatalog { get;  set; }
    }
}
