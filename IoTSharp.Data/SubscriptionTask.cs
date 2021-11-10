using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class SubscriptionTask
    {
        public Guid BindId { get; set; }

        public Subscription Subscription { get; set; }

        public RuleTaskExecutor RuleTaskExecutor { get; set; }

        public int Status { get; set; }
    }
}
