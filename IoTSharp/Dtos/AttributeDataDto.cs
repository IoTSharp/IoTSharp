using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataType = IoTSharp.Data.DataType;

namespace IoTSharp.Dtos
{
    public class AttributeDataDto
    {
        public string KeyName { get; set; }

        public DataSide DataSide { get; set; }

        public DateTime DateTime { get; set; }
 
        public  object Value{ get; set; }
        public  DataType DataType { get;   set; }
    }
}
