using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoT.Things.ModBus
{
    public class AppSettings
    {
       
        public Uri BrokerUri { get;  set; }
        public string AccessToken { get;   set; }
        public List<string> ModBusList { get; set; }
        public string DeviceId { get;   set; }
    }
    public class ModBusConfig
    {
        public string DataType { get; set; }
        public string ValueType { get; set; }
        public string KeyNameOrPrefix { get; set; }
        public string Address { get; set; }
        public int Lenght { get; set; }
        public Uri ModBusUri { get; set; }
        public int Interval { get;  set; }
    }
}