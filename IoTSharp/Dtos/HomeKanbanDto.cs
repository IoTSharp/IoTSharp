using Microsoft.AspNetCore.Identity;
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
        public int AttributesDataCount { get; set; }
        public int DeviceCount { get; set; }
        public int AlarmsCount { get; internal set; }
        public  int  UserCount { get;  set; }
        public int ProduceCount { get; internal set; }
        public int RulesCount { get; internal set; }
    }
}
