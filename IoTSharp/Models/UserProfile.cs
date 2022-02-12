using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models
{
    public class UserProfile
    {
        public string[] Roles { get; set; }
        public Guid Id { get; set; }
        public string Email{ get; set; }
        public Guid Comstomer { get; set; }
        public Guid Tenant { get; set; }
        public string Name { get; set; }
    }
}
