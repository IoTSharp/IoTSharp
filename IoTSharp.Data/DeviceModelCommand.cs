using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class DeviceModelCommand
    {
        [Key]
        public Guid CommandId { get; set; }

        public string CommandTitle { get; set; }
        public string CommandI18N { get; set; }

        public int CommandType { get; set; }

        public string CommandParams { get; set; }

        public string CommandName { get; set; }
        public string CommandTemplate { get; set; }

        public Guid DeviceModelId { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid Creator { get; set; }
        public int CommandStatus { get; set; }

        public DeviceModel DeviceModel { get; set; }
    }
}