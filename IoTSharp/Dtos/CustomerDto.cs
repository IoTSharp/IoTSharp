using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class CustomerDto : Customer
    {
        public Guid TenantId { get; set; }
    }
}