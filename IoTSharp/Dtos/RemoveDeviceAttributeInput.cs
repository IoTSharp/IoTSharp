using IoTSharp.Contracts;
using System;

namespace IoTSharp.Dtos
{
    public class RemoveDeviceAttributeInput
    {
        public Guid DeviceId { get; set; }
        public string KeyName { get; set; }
        
        public DataSide DataSide { get; set; }
    }
}
