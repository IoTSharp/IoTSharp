using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class SubscriptionEvent
    {

        public Guid EventnId { get; set; }

        public string EventName { get; set; }

        public string EventDesc { get; set; }

        public string EventNameSpace { get; set; }

        public int EventStatus { get; set; }

        public int Type { get; set; }

        public string EventParam { get; set; }
        public string EventTag { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid Creator { get; set; }
    }
}
