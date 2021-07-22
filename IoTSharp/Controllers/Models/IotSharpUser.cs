using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Controllers.Models
{
    public class IotSharpUser:IdentityUser
    {
        public int Type { get; set; }
        public string CustomerId { get; set; }
        public string TenantId { get; set; }
        public DateTime CraeteDateTime { get; set; }
    }
}
