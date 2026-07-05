using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    [Obsolete("DeviceModel 已合并到 Product，保留该类型仅用于历史数据库结构。")]
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
