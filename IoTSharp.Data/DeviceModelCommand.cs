using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    [Obsolete("DeviceModelCommand 已合并到 ProductCommand，保留该类型仅用于历史数据库结构。")]
    public class DeviceModelCommand
    {
        [Key]
        public Guid CommandId { get; set; }

        public string CommandTitle { get; set; }

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
