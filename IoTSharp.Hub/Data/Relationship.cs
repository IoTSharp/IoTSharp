using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class Relationship
    {
        [Key]
        public Guid UseId { get; set; }
        public Guid CustomerId { get; set; }

        public Guid TenantId { get; set; }
    }
}
