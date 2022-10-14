using System;
using IoTSharp.Contracts;

namespace IoTSharp.Dtos
{



    public class ProduceDataEditDto
    {
        public Guid produceId { get; set; }
        public ProduceDataItemDto[] ProduceData { get; set; }
    }



    public class ProduceDataItemDto
    {
        public string KeyName { get; set; }
        public DataSide DataSide { get; set; }
        public DataType Type { get; set; }

    }

}
