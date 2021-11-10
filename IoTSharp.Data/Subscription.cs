using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
   public class Subscription
    {

        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string SubscriptionDesc { get; set; }

        public string SubscriptionNameSpace{ get; set; }

        public string SubscriptionNameStatus { get; set; }

        public string SubscriptionParam { get; set; }
        public string SubscriptionTag { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid Creator { get; set; }
    }
}
