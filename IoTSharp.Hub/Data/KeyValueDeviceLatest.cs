using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public class KeyValueDeviceLatest : KeyValueSharedSide
    {
        [Required]
        [EnumDataType(typeof(KeyValueScope))]
        public KeyValueScope Scope { get; set; }
    }
}
