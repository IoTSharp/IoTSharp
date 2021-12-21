using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class DevicePostDto
    {
        public string Name { get; set; }
        public DeviceType DeviceType { get; set; }
        public Guid? DeviceModelId { get; set; }

        public int Timeout { get; set; }
        public IdentityType IdentityType { get; set; }
    }
}
