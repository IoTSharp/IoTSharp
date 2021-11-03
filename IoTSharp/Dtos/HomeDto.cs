using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamitey.DynamicObjects;
using IoTSharp.Data;

namespace IoTSharp.Dtos
{
    public class HomeDto
    {

        public  int DeviceCount { get; set; }

        public int EventsCount { get; set; }

        public List<Device> Devices { get; set; }

        public List<FlowRule> Rules { get; set; }
    }
}
