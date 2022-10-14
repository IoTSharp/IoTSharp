using System;

namespace IoTSharp.Dtos
{
    public class CreateDeviceByProduceDto
    {
        public string DeviceName { get; set; }

        public Guid ProduceId { get; set; }
    }
}
