using IoTSharp.Contracts;
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
        /// ICON file full path 
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 网关类型  根据不通网关来处理相关配置
        /// </summary>
        public GatewayType GatewayType { get; set; } = GatewayType.Unknow;

        /// <summary>
        /// 网关配置信息， 如果是Unknow 则不使用， 如果是自定义 ，则这里是json字符串。 
        /// 如果是其他对应的网关， 则这里是 对应的配置文件名。 
        /// </summary>
        public string GatewayConfiguration { get; set; } = string.Empty;
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
        /// <summary>
        /// 默认认证类型
        /// </summary>
        [EnumDataType(typeof(IdentityType))]
        public IdentityType DefaultIdentityType { get; set; } = IdentityType.AccessToken;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 默认属性
        /// </summary>
        public List<ProduceData> DefaultAttributes { get; set; }
        /// <summary>
        /// 所属设备
        /// </summary>
        public List<Device> Devices { get; set; }
        /// <summary>
        /// 默认设备类型
        /// </summary>
        public DeviceType DefaultDeviceType { get; set; }
        /// <summary>
        /// 产品字典
        /// </summary>
        public List<ProduceDictionary> Dictionaries { get; set; }
        public string ProduceToken { get; set; }

        public bool Deleted { get; set; }

    }
}
