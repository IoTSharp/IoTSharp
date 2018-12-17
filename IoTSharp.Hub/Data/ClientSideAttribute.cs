using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{

    public class ClientSideAttribute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        [EnumDataType(typeof(AttributeType))]
        public AttributeType Type { get; set; }
        public bool Value_Boolean { get; set; }
        public string Value_String { get; set; }
        public long Value_Long { get; set; }
        public double Value_Double { get; set; }
        public string Value_Json { get; set; }
        public string Value_XML { get; set; }
        public byte[] Value_Binary { get; set; }
    }
}
