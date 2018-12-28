using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Hub.Data
{
    public class AttributeData
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string KeyName { get; set; }
        [Required]
        [EnumDataType(typeof(DataType))]
        public DataType Type { get; set; }
        public DateTime DateTime { get; set; }

        public bool Value_Boolean { get; set; }
        public string Value_String { get; set; }
        public long Value_Long { get; set; }
        public double Value_Double { get; set; }
        public string Value_Json { get; set; }
        public string Value_XML { get; set; }
        public byte[] Value_Binary { get; set; }
    }
}
