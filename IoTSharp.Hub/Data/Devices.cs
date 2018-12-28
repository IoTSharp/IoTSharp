using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class Device
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
       
        public string Type { get; set; }

        public Tenant  Tenant { get; set; }

        public Customer Customer   { get; set; }
       
        public virtual List<KeyValueServerSide> ServerSide{ get; set; }
        public virtual List<KeyValueSharedSide> SharedSide { get; set; }
        public  virtual List<KeyValueClientSide> ClientSide { get; set; }

        public virtual List<KeyValueDeviceLatest> DeviceLatest { get; set; }
        public virtual List<KevValueTelemetry> Telemetry { get;  set; }
    }
}
