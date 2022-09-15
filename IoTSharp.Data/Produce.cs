using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{

    public class Produce
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 超时时间 秒数
        /// </summary>
        public int DefaultTimeout { get; set; } = 300;


        /// <summary>
        /// 租户
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public Customer Customer { get; set; }

        [EnumDataType(typeof(IdentityType))]
        public IdentityType DefaultIdentityType { get; set; } = IdentityType.AccessToken;
        public string Description { get; set; }
        public List<ProduceData> DefaultAttributes { get; set; }

        public List<Device> Devices { get; set; }
        public DeviceType DefaultDeviceType { get; set; }
    }
}
