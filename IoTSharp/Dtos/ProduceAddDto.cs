using IoTSharp.Contracts;
using IoTSharp.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace IoTSharp.Dtos
{
    public class ProduceAddDto
    {
  
        public Guid Id { get; set; } 

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
           [EnumDataType(typeof(GatewayType))]
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
        public Guid Tenant { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public Guid Customer { get; set; }
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
        /// 默认设备类型
        /// </summary>
        public DeviceType DefaultDeviceType { get; set; }
      
    }
}
