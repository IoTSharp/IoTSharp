using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class BaseEvent
    {

        [Key]
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string EventDesc { get; set; }
        public int EventStaus { get; set; }
        public EventType Type { get; set; }
        public string MataData { get; set; }
        public Guid Creator { get; set; }
        public FlowRule FlowRule { get; set; }
        public string Bizid { get; set; }
        public DateTime CreaterDateTime { get; set; }
        public string BizData { get; set; }

        public Tenant Tenant { get; set; }
    
        public Customer Customer { get; set; }


    }
}
