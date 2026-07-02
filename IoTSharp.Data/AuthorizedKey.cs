using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AuthorizedKey : IJustMy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public string AuthToken { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Tenant Tenant { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Customer Customer { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Device> Devices { get; set; }

        public bool Deleted { get; set; }
    }
}
