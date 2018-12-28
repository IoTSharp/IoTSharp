using Microsoft.AspNetCore.Identity;
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
        public Guid Id { get; set; }
        public IdentityUser Identity { get; set; }
        public Tenant Tenant { get; set; }

        public Customer Customer { get; set; }
    }
}
