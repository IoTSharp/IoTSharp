using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Address { get; set; }
        public int ZipCode { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Device> Devices { get; set; }


    }
}
