using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class DeviceRule
    {
        [Key]
        public Guid DeviceRuleId { get; set; }
        public Device Device { get; set; }
        public FlowRule FlowRule { get; set; }
        public Guid ConfigUser { get; set; }
        public DateTime ConfigDateTime { get; set; }

        public int EnableTrace { get; set; } //是否开启设备在规则链当中的链路跟踪
    }
}