using System;

namespace IoTSharp.Dtos
{
    public class CreateDeviceByProductDto
    {
        public string DeviceName { get; set; }

        public Guid ProductId { get; set; }
    }
}
