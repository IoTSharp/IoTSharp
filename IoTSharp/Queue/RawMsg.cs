using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{

  
    public class RawMsg
    {
        public MsgType MsgType { get; set; }
  
        public Guid DeviceId { get;   set; }
        public Dictionary<string, object> MsgBody { get; internal set; }
        public DataSide DataSide { get; internal set; }
        public DataCatalog DataCatalog { get; internal set; }
    }
}
