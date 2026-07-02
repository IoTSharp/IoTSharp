using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Contracts;
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
        /// 超时时间 秒数 
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 所有者
        /// </summary>
        public Gateway Owner { get; set; }

        /// <summary>
        /// 拥有者显示名称。网关子设备显示网关名称，产品认证设备显示产品名称。
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// 拥有者类型。
        /// </summary>
        public string OwnerType { get; set; }


        public IdentityType IdentityType { get; set; }


        public string IdentityValue { get; set; }
        public string IdentityId { get; set; }
        /// <summary>
        /// 旧版活跃标记。仅保留兼容，不作为控制台在线状态展示。
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// 旧版最后活跃时间。仅保留兼容，不作为控制台在线状态展示。
        /// </summary>
        public DateTime? LastActivityDateTime { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Connected { get; set; }
        /// <summary>
        /// 最后上线时间
        /// </summary>
        public DateTime? LastConnectDateTime { get; set; }
        /// <summary>
        /// 最后离线时间
        /// </summary>
        public DateTime? LastDisconnectDateTime { get; set; }
        public string TenantName { get; set; }
        public string CustomerName { get; set; }
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
