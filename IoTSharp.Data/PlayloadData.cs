using IoTSharp.Contracts;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class PlayloadData
    {
        public DateTime ts { get; set; } = DateTime.UtcNow;
        public Guid DeviceId { get;   set; }
        public Dictionary<string, object> MsgBody { get;  set; }
        public DataSide DataSide { get;  set; }
        public DataCatalog DataCatalog { get;  set; }
    }
}
