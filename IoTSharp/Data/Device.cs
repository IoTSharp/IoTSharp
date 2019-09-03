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
        public DeviceType DeviceType { get; set; }
        public Gateway Owner { get; set; }
        public Tenant Tenant { get; set; }

        public Customer Customer { get; set; }

    }
}