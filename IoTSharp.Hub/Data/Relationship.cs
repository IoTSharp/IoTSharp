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
        public virtual IdentityUser Identity { get; set; }
        public virtual Tenant Tenant { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
