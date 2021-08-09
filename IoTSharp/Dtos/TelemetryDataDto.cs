using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class TelemetryDataDto
    {
        public string KeyName { get; set; }

        public DateTime DateTime { get; set; }
        public Data.DataType DataType { get; set; }
        public object Value { get; set; }
    }
}
