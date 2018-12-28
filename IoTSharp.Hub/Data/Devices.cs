using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class Device
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
       
        public string Type { get; set; }

        public Tenant  Tenant { get; set; }

        public Customer Customer   { get; set; }
        
        public List<KeyValueServerSide> ServerSideKeyValue { get; set; }
        public List<KeyValueSharedSide> SharedSideKeyValue { get; set; }
        public List<KeyValueClientSide> ClientSideKeyValue { get; set; }


    }
}
