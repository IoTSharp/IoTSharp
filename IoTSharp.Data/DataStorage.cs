using IoTSharp.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataType = IoTSharp.Contracts.DataType;

namespace IoTSharp.Data
{
    public interface IDataStorage
    {
        public Guid DeviceId { get; set; }

        public string KeyName { get; set; }

        public DateTime DateTime { get; set; }

        public DataSide DataSide { get; set; }

        public DataType Type { get; set; }

        public bool? Value_Boolean { get; set; }
        public string Value_String { get; set; }
        public long? Value_Long { get; set; }
        public DateTime? Value_DateTime { get; set; }
        public double? Value_Double { get; set; }
        public string Value_Json { get; set; }
        public string Value_XML { get; set; }
        public byte[] Value_Binary { get; set; }
    }
    public class DataStorage: IDataStorage
    {
        [EnumDataType(typeof(DataCatalog)), Column(Order = 0)]
        public DataCatalog Catalog { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore, Column(Order = 1)]
        public Guid DeviceId { get; set; }

        [Column(Order = 2)]
        public string KeyName { get; set; }

        [Column(Order = 3 )]
        public DateTime DateTime { get; set; }

        [Column(Order = 4)]
        [EnumDataType(typeof(DataSide))]
        public DataSide DataSide { get; set; } = DataSide.AnySide;

        [Column(Order = 5)]
        [EnumDataType(typeof(Contracts.DataType))]
        public Contracts.DataType Type { get; set; }

        public bool? Value_Boolean { get; set; }
        public string Value_String { get; set; }
        public long? Value_Long { get; set; }
   
        public DateTime? Value_DateTime { get; set; }
        public double? Value_Double { get; set; }
        public string Value_Json { get; set; }
        public string Value_XML { get; set; }
        public byte[] Value_Binary { get; set; }
    }
}