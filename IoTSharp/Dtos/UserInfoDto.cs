using IoTSharp.Hub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Dtos
{
    public class UserInfoDto
    {
        public Customer Customer { get; set; }
        public Tenant Tenant { get; set; }
    }
}