using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class DeviceModel
    {
        [Key]
        public Guid DeviceModelId { get; set; }

        public string ModelName { get; set; }
        public string ModelDesc { get; set; }
        public int ModelStatus { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid Creator { get; set; }
        public List<DeviceModelCommand> DeviceModelCommands { get; set; }
    }
}