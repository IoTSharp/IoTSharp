using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// Edge 运行节点，承载 Gateway 注册、心跳和能力上报后的平台侧状态。
    /// </summary>
    public class EdgeNode : IJustMy
    {
        /// <summary>
        /// Edge 节点标识，与承载接入凭证的 Gateway 设备保持一一对应。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 关联的 Gateway 设备 ID，用于复用现有设备身份、权限和任务寻址。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 关联的 Gateway 设备。
        /// </summary>
        public Device Gateway { get; set; }

        /// <summary>
        /// 管理侧显示名称，默认来自 Gateway 设备名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 运行时类型，例如 gateway、collector。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 运行时实例名称。
        /// </summary>
        public string RuntimeName { get; set; }

        /// <summary>
        /// 运行时版本号。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 执行端实例 ID，用于同一 Gateway 下区分具体运行实例。
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 运行平台描述。
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 主机名。
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 最近上报的 IP 地址。
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 运行状态，例如 Registered、Running、Degraded。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 最近一次健康状态，未上报时为空。
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// 最近一次上报的运行时长，单位秒。
        /// </summary>
        public long? UptimeSeconds { get; set; }

        /// <summary>
        /// 最近一次注册时间。
        /// </summary>
        public DateTime? LastRegistrationDateTime { get; set; }

        /// <summary>
        /// 最近一次心跳时间。
        /// </summary>
        public DateTime? LastHeartbeatDateTime { get; set; }

        /// <summary>
        /// 最近一次能力声明 JSON。
        /// </summary>
        public string Capabilities { get; set; }

        /// <summary>
        /// 注册或能力上报携带的元数据 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 最近一次心跳指标 JSON。
        /// </summary>
        public string Metrics { get; set; }

        /// <summary>
        /// 当前节点是否被删除。
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 节点创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 节点状态最后更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 租户。
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 租户导航属性。
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户。
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// 客户导航属性。
        /// </summary>
        public Customer Customer { get; set; }
    }
}
