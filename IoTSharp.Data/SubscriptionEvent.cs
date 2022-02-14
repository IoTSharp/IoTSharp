using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    public class SubscriptionEvent : IJustMy
    {
        [Key]
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string EventDesc { get; set; }
        public string EventNameSpace { get; set; }
        public int EventStatus { get; set; }
        public int Type { get; set; }
        public string EventParam { get; set; }
        public string EventTag { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid Creator { get; set; }
        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}