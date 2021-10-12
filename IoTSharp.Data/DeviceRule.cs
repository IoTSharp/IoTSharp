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
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long DeviceRuleId { get; set; }
        public long FlowId { get; set; }
        public Guid DeviceId { get; set; }
        public Guid ConfigUser  { get; set; }
        public DateTime ConfigDateTime { get; set; }
    }
}
