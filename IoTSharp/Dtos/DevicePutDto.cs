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
        public IdentityType IdentityType { get; set; }

        /// <summary>
        /// 可选的产品模板标识。修改时必须属于当前租户和客户。
        /// </summary>
        public Guid? ProductId { get; set; }

    }
}
