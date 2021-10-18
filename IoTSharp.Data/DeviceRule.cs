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
    
        public Guid DeviceRuleId { get; set; }

      

      public Device Device { get; set; }
      public FlowRule FlowRule { get; set; }
        public Guid ConfigUser { get; set; }
        public DateTime ConfigDateTime { get; set; }
    }
}
