using IoTSharp.Hub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Dtos
{
    public class CustomerDto : Customer
    {
        public Guid TenantID { get; set; }
    }
}