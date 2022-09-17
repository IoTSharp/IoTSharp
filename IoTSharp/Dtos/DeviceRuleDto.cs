using System;
using IoTSharp.Contracts;
using IoTSharp.Data;

namespace IoTSharp.Dtos
{
    public class DeviceRuleDto
    {
        public Guid Id { get; set; } 

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


        public int Status { get; set; }

        public int EnableTrace { get; set; }


    }
}
