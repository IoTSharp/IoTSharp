using System;

namespace IoTSharp.Dtos
{
    public class AttributeLatestByKeyNameAndDeviceIdDto
    {
        public Guid [] deviceIds { get; set; }
        public string[] keyNames { get; set; }
    }
}
