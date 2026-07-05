using System;
using System.Collections.Generic;

namespace IoTSharp.Contracts
{
    /// <summary>
    /// EdgeNode 跨仓合同版本。新增版本只能追加，不得重定义既有版本语义。
    /// </summary>
    public static class EdgeNodeContractVersions
    {
        /// <summary>
        /// EdgeNode 平台状态快照合同版本。
        /// </summary>
        public const string EdgeNodeV1 = "edge-node-v1";

        /// <summary>
        /// Edge 运行时状态快照合同版本。
        /// </summary>
        public const string EdgeRuntimeStatusV1 = "edge-runtime-status-v1";

        /// <summary>
        /// Edge 能力快照合同版本。
        /// </summary>
        public const string EdgeCapabilityV1 = "edge-capability-v1";

        /// <summary>
        /// 运行时注册、心跳和能力上报的现有 wire 版本。
        /// </summary>
        public const string EdgeRuntimeV1 = "edge-v1";

        /// <summary>
        /// 采集运行时配置拉取合同版本。
        /// </summary>
        public const string CollectionConfigV1 = "collection-config-v1";

        /// <summary>
        /// M2 #027 固化前曾使用的采集配置合同版本，仅用于识别历史载荷。
        /// </summary>
        [Obsolete("请使用 CollectionConfigV1。该值只用于兼容 M2 #027 之前的历史载荷。")]
        public const string LegacyEdgeCollectionV1 = "edge-collection-v1";

        /// <summary>
        /// Edge 任务请求与回执合同版本。
        /// </summary>
        public const string EdgeTaskV1 = "edge-task-v1";
    }

    /// <summary>
    /// 已发布的 Edge 运行时类型名称。
    /// </summary>
    public static class EdgeRuntimeTypes
    {
        /// <summary>
        /// 同时承担 Gateway 接入职责的边缘运行时。
        /// </summary>
        public const string Gateway = "gateway";

        /// <summary>
        /// 仅承担采集执行职责的边缘运行时。
        /// </summary>
        public const string Collector = "collector";
    }

    /// <summary>
    /// 平台识别的 EdgeNode 运行状态名称。
    /// </summary>
    public static class EdgeNodeStatusNames
    {
        /// <summary>
        /// 平台已创建节点，但执行端尚未注册。
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// 执行端完成注册。
        /// </summary>
        public const string Registered = "Registered";

        /// <summary>
        /// 执行端正在运行。
        /// </summary>
        public const string Running = "Running";

        /// <summary>
        /// 执行端仍在线，但存在降级能力或局部故障。
        /// </summary>
        public const string Degraded = "Degraded";

        /// <summary>
        /// 执行端离线。
        /// </summary>
        public const string Offline = "Offline";

        /// <summary>
        /// 执行端报告失败状态。
        /// </summary>
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Edge 运行时注册请求。
    /// </summary>
    public class EdgeRegistrationDto
    {
        /// <summary>
        /// 合同版本。旧执行端未传该字段时按 edge-v1 处理。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeRuntimeV1;

        /// <summary>
        /// 运行时类型，例如 gateway 或 collector。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例名称。未传时平台使用 Gateway 名称。
        /// </summary>
        public string RuntimeName { get; set; } = string.Empty;

        /// <summary>
        /// 运行时软件版本。
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 安装实例标识，用于区分同一宿主上的不同运行时实例。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 运行平台描述。
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// 宿主机名。
        /// </summary>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// 最近上报的 IP 地址。
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 非敏感部署元数据，不承载密钥、日志或高频指标。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// Edge 运行时注册结果。
    /// </summary>
    public class EdgeRegistrationResultDto
    {
        /// <summary>
        /// 合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeRuntimeV1;

        /// <summary>
        /// 承载 EdgeNode 身份的 Gateway 设备 ID，保留该字段以兼容既有执行端。
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid EdgeNodeId { get; set; }

        /// <summary>
        /// 承载接入凭证的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 平台管理侧名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 本次注册使用的接入令牌。
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 平台配置的心跳超时时间，单位秒。
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 平台确认注册的 UTC 时间。
        /// </summary>
        public DateTime RegisteredAt { get; set; }
    }

    /// <summary>
    /// Edge 运行时心跳请求。
    /// </summary>
    public class EdgeHeartbeatDto
    {
        /// <summary>
        /// 合同版本。旧执行端未传该字段时按 edge-v1 处理。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeRuntimeV1;

        /// <summary>
        /// 执行端采集到心跳的 UTC 时间。未传时平台使用接收时间。
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// 执行端上报的运行状态。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 最近一次健康判断。
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// 执行端运行时长，单位秒。
        /// </summary>
        public long? UptimeSeconds { get; set; }

        /// <summary>
        /// 最近上报的 IP 地址。
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 运行时类型。用于执行端在心跳中修正注册时的运行时信息。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 低频运行指标快照，不承载大体积日志。
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = [];
    }

    /// <summary>
    /// Edge 运行时能力上报请求。
    /// </summary>
    public class EdgeCapabilityReportDto
    {
        /// <summary>
        /// 合同版本。旧执行端未传该字段或继续传 edge-v1 时仍按兼容能力上报处理。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeCapabilityV1;

        /// <summary>
        /// 执行端生成能力快照的 UTC 时间。未传时平台使用接收时间。
        /// </summary>
        public DateTime? ReportedAt { get; set; }

        /// <summary>
        /// 支持的南向协议或协议族。
        /// </summary>
        public string[] Protocols { get; set; } = [];

        /// <summary>
        /// 归一化后的协议枚举能力，用于与采集模板进行静态匹配。
        /// </summary>
        public CollectionProtocolType[] SupportedProtocols { get; set; } = [];

        /// <summary>
        /// 支持的点位源类型，例如 coil、holding-register、opcua-node。
        /// </summary>
        public string[] SupportedPointTypes { get; set; } = [];

        /// <summary>
        /// 支持的值转换能力。
        /// </summary>
        public CollectionTransformType[] SupportedTransforms { get; set; } = [];

        /// <summary>
        /// 支持的上报触发能力。
        /// </summary>
        public ReportTriggerType[] SupportedReportTriggers { get; set; } = [];

        /// <summary>
        /// 支持的平台能力标识。
        /// </summary>
        public string[] Features { get; set; } = [];

        /// <summary>
        /// 支持接收的任务能力标识。
        /// </summary>
        public string[] Tasks { get; set; } = [];

        /// <summary>
        /// 任务能力的结构化声明，补充进度、取消和任务合同版本等约束。
        /// </summary>
        public EdgeTaskCapabilityDto[] TaskCapabilities { get; set; } = [];

        /// <summary>
        /// 执行端声明可兼容的云边合同版本。
        /// </summary>
        public EdgeContractCompatibilityDto[] CompatibleContracts { get; set; } = [];

        /// <summary>
        /// 非敏感能力扩展元数据。
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = [];
    }

    /// <summary>
    /// Edge 运行时状态快照，收敛注册、心跳、实例、主机、健康和指标等运行态字段。
    /// </summary>
    public class EdgeRuntimeStatusDto
    {
        /// <summary>
        /// 状态快照合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeRuntimeStatusV1;

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid EdgeNodeId { get; set; }

        /// <summary>
        /// 承载接入凭证的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// Gateway 设备活动状态，由平台接入状态推导。
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 最近一次平台记录到的活动时间。
        /// </summary>
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// 运行时类型，例如 gateway 或 collector。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例名称。
        /// </summary>
        public string RuntimeName { get; set; } = string.Empty;

        /// <summary>
        /// 运行时软件版本。
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 运行平台描述。
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// 宿主机名。
        /// </summary>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// 最近上报的 IP 地址。
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 运行状态，例如 Registered、Running、Degraded 或 Failed。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 最近一次健康状态，未上报时为空。
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// 最近一次上报的运行时长，单位秒。
        /// </summary>
        public long? UptimeSeconds { get; set; }

        /// <summary>
        /// 最近一次心跳时间。
        /// </summary>
        public DateTime? LastHeartbeatDateTime { get; set; }

        /// <summary>
        /// 最近一次注册时间。
        /// </summary>
        public DateTime? LastRegistrationDateTime { get; set; }

        /// <summary>
        /// 最近一次状态持久化更新时间。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 注册或能力上报携带的非敏感部署元数据。
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = [];

        /// <summary>
        /// 最近一次心跳携带的低频运行指标。
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = [];
    }

    /// <summary>
    /// EdgeNode 平台状态快照。
    /// </summary>
    public class EdgeNodeDto
    {
        /// <summary>
        /// 状态快照合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeNodeV1;

        /// <summary>
        /// EdgeNode ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 承载接入凭证的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 管理侧名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gateway 接入令牌。
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 平台配置的心跳超时时间，单位秒。
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 设备活动状态。
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 最近一次活动时间。
        /// </summary>
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例名称。
        /// </summary>
        public string RuntimeName { get; set; } = string.Empty;

        /// <summary>
        /// 运行时版本。
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 运行平台描述。
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// 主机名。
        /// </summary>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// 最近上报的 IP 地址。
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 运行状态。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 最近一次健康状态。
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// 最近一次上报的运行时长，单位秒。
        /// </summary>
        public long? UptimeSeconds { get; set; }

        /// <summary>
        /// 最近一次心跳时间。
        /// </summary>
        public DateTime? LastHeartbeatDateTime { get; set; }

        /// <summary>
        /// 最近一次注册时间。
        /// </summary>
        public DateTime? LastRegistrationDateTime { get; set; }

        /// <summary>
        /// 最近一次能力声明 JSON。
        /// </summary>
        public string Capabilities { get; set; } = string.Empty;

        /// <summary>
        /// 正式能力快照。扁平 Capabilities 字符串保留用于兼容既有控制台和执行端。
        /// </summary>
        public EdgeCapabilityDto Capability { get; set; } = new();

        /// <summary>
        /// 注册或能力上报携带的元数据 JSON。
        /// </summary>
        public string Metadata { get; set; } = string.Empty;

        /// <summary>
        /// 最近一次心跳指标 JSON。
        /// </summary>
        public string Metrics { get; set; } = string.Empty;

        /// <summary>
        /// 最近任务回执状态。
        /// </summary>
        public string LastTaskStatus { get; set; } = string.Empty;

        /// <summary>
        /// 最近任务回执上报时间。
        /// </summary>
        public DateTime? LastReceiptDateTime { get; set; }

        /// <summary>
        /// 正式运行时状态快照。扁平字段保留用于兼容既有控制台和执行端。
        /// </summary>
        public EdgeRuntimeStatusDto RuntimeStatus { get; set; } = new();
    }
}
