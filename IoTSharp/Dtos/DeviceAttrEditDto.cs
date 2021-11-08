using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class DeviceAttrEditDto
    {
        public Dictionary<string, object> serverside { get; set; }

        public Dictionary<string, object> anyside { get; set; }
    }
}
