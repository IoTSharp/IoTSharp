using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Hub.Data
{
    public class DataStorage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string KeyName { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [Required]
        public Device Device { get; set; }

        [Required]
        [EnumDataType(typeof(DataSide))]
        public DataSide DataSide { get; set; } = DataSide.AnySide;

        [Required]
        [EnumDataType(typeof(DataCatalog))]
        public DataCatalog Catalog { get; set; }

        [Required]
        [EnumDataType(typeof(DataType))]
        public DataType Type { get; set; }

        public DateTime DateTime { get; set; }

        public bool Value_Boolean { get; set; }
        public string Value_String { get; set; }
        public long Value_Long { get; set; }
        public DateTime Value_DateTime { get; set; }
        public double Value_Double { get; set; }
        public string Value_Json { get; set; }
        public string Value_XML { get; set; }
        public byte[] Value_Binary { get; set; }
    }
}