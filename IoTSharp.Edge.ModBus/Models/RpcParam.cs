using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoT.Things.ModBus.Models
{
    public class RpcParam<T>
    {
        public string Address { get; set; }
        public T Value { get; set; }
    }
}
