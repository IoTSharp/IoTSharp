using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class Relationship
    {

        [Key]
        public Guid Id { get; set; }
        public IdentityUser IdentityUser  { get; set; }
        public virtual Tenant Tenant { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
