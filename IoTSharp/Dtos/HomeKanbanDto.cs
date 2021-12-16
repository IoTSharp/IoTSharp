using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class HomeKanbanDto
    {

        public int EventCount { get; set; }
        public int OnlineDeviceCount { get; set; }
        public int TelemetryDataCount { get; set; }
        public int DeviceCount { get; set; }



        public int[] DeviceCountData { get; set; }
    }
}
