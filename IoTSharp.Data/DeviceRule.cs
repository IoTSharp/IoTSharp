using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class DeviceRule
    {

        [Key]
        public int DeviceRuleId { get; set; }
        public Guid Id { get; set; }

        //public long RuleId { get; set; }
        //public Guid DeviceId { get; set; }
        public Device Device { get; set; }
        public FlowRule FlowRule { get; set; }
        public Guid ConfigUser { get; set; }
        public DateTime ConfigDateTime { get; set; }
    }
}
