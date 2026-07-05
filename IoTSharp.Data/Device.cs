using IoTSharp.Contracts;
using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// 设备
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 设备ID
        /// </summary>
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
        /// 租户
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid? TenantId { get; set; }

        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid? CustomerId { get; set; }

        public Customer Customer { get; set; }


        /// <summary>
        /// 历史设备模型ID。活动 API 不再使用，命令和能力定义由 Product 承载。
        /// </summary>
        [Obsolete("DeviceModel 已合并到 Product，活动代码不应继续使用该字段。")]
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid? DeviceModelId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Product Product { get; set; }

        public DeviceIdentity DeviceIdentity { get; set; }

        public bool Deleted { get; set; }

    }
}
