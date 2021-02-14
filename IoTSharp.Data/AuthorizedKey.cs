using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class AuthorizedKey: IJustMy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public string AuthToken { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Tenant Tenant { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Customer Customer { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public List<Device> Devices { get; set; }
    }
}
