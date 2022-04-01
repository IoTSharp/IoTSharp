using System;

namespace IoTSharp.Models
{
    public class ModelOriginatorSearch
    {

        public int OriginatorType { get; set; }
        public string OriginatorName { get; set; }

    }

    public class ModelOriginatorItem
    {

        public Guid Id { get; set; }
        public string Name { get; set; }

    }

}
