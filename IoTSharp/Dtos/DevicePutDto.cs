using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class DevicePutDto
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public IdentityType IdentityType { get; set; }
    }
}
