using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class KeyValueSharedSide : KeyValue
    {
        [Key]
        public Device Device { get; set; }
    }
}
