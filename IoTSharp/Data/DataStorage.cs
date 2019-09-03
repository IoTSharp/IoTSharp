using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IoTSharp.Data
{
    public class DataStorage
    {
        [EnumDataType(typeof(DataCatalog)), Column(Order = 0)]
        public DataCatalog Catalog { get; set; }

        [Newtonsoft.Json.JsonIgnore, Column(Order = 1)]
        public Guid DeviceId { get; set; }

        [Column(Order = 2)]
        public string KeyName { get; set; }

        [Column(Order = 3)]
        public DateTime DateTime { get; set; }

        [Column(Order = 4)]
        [EnumDataType(typeof(DataSide))]
        public DataSide DataSide { get; set; } = DataSide.AnySide;

        [Column(Order = 5)]
        [EnumDataType(typeof(DataType))]
        public DataType Type { get; set; }

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