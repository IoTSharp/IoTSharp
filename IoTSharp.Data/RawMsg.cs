using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{

    public class DeviceStatus
    {
        public Guid DeviceId { get; set; }
        public bool Status { get; set; }
    }
  
    public class RawMsg
    {
        public MsgType MsgType { get; set; }
  
        public Guid DeviceId { get;   set; }
        public Dictionary<string, object> MsgBody { get;  set; }
        public DataSide DataSide { get;  set; }
        public DataCatalog DataCatalog { get;  set; }
    }
}
