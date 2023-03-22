using IoTSharp.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using IoTSharp.Contracts;

namespace IoTSharp.Dtos
{
    public class ProduceDto
    {
        public Guid Id { get; set; }

    
        public string Name { get; set; }

    
        public int DefaultTimeout { get; set; } = 300;

 
        [EnumDataType(typeof(IdentityType))]
        public IdentityType DefaultIdentityType { get; set; } = IdentityType.AccessToken;
        public string Description { get; set; }

        public List<Device> Devices { get; set; }
        /// <summary>
        /// 默认设备类型
        /// </summary>
        public DeviceType DefaultDeviceType { get; set; }

    }
}
