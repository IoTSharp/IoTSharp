using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;

namespace IoTSharp.Dtos
{
    public class DeviceDetailDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; set; }
        /// <summary>
        /// 在线
        /// </summary>
        public bool Online { get; set; }
        /// <summary>
        /// 最后一次活跃时间
        /// </summary>
        public DateTime LastActive { get; set; }
        /// <summary>
        /// 超时时间 秒数 
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 所有者
        /// </summary>
        public Gateway Owner { get; set; }
        /// <summary>
        /// 租户
        /// </summary>
        public Tenant Tenant { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public Customer Customer { get; set; }

        public IdentityType IdentityType { get; set; }


        public string IdentityValue { get; set; }
        public string IdentityId { get; set; }

#nullable enable
        public DeviceModel? Model { get; set; }

#nullable disable
    }
}
