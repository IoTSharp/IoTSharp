using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class DevicePutDto
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; set; }
        public int Timeout { get; set; }
        public Guid? DeviceModelId { get; set; }
        public IdentityType IdentityType { get; set; }
    }
}
