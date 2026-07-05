using System;
using IoTSharp.Contracts;

namespace IoTSharp.Dtos
{



    public class ProductDataEditDto
    {
        public Guid ProductId { get; set; }
        public ProductDataItemDto[] ProductData { get; set; }
    }



    public class ProductDataItemDto
    {
        public string KeyName { get; set; }
        public DataSide DataSide { get; set; }
        public DataType Type { get; set; }

    }

}
