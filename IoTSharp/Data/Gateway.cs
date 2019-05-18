using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Gateway: Device
    {
        [Newtonsoft.Json.JsonIgnore]
        public List<Device> Children { get; set; }
    }
}
