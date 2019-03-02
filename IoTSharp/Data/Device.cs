using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Device
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public string Type { get; set; }

        public Tenant Tenant { get; set; }

        public Customer Customer { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual List<AttributeData> AttributeData { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual List<TelemetryData> TelemetryData { get; set; }

        public virtual List<AttributeLatest> AttributeLatest { get; set; }

        public virtual List<TelemetryLatest> TelemetryLatest { get; set; }
    }
}