using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AuditLog
    {
        public Guid Id  { get; set; } = Guid.NewGuid();
        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Guid ObjectID { get; set; }
        public string ObjectName { get; set; }
        public ObjectType ObjectType { get; set; } = ObjectType.Unknow;
        public string ActionName { get; set; }
        public string ActionData { get; set; }
        public string ActionResult { get; set; }
        public DateTime ActiveDateTime { get; set; } = DateTime.Now;

    }
}
