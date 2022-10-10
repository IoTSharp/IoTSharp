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
 

        public IdentityType IdentityType { get; set; }


        public string IdentityValue { get; set; }
        public string IdentityId { get; set; }
        /// <summary>
        /// 是否活动
        /// </summary>
        public bool Active { get;  set; }
        /// <summary>
        /// 最后活动
        /// </summary>
        public DateTime? LastActivityDateTime { get;  set; }
        public string TenantName { get;   set; }
        public string CustomerName { get;  set; }
        public Guid TenantId { get;  set; }
        public Guid CustomerId { get;  set; }
    }
}
