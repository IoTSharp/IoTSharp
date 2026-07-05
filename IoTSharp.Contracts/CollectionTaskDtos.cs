using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionProtocolType
{
    Unknown = 0,
    Modbus = 1,
    OpcUa = 2,
    Bacnet = 3,
    IEC104 = 4,
    Mqtt = 5,
    Custom = 99,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionTargetType
{
    Telemetry = 1,
    Attribute = 2,
    AlarmInput = 3,
    CommandFeedback = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionValueType
{
    Boolean = 1,
    Int32 = 2,
    Int64 = 3,
    Double = 4,
    Decimal = 5,
    String = 6,
    Enum = 7,
    Json = 8,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionTransformType
{
    Scale = 1,
    Offset = 2,
    Expression = 3,
    EnumMap = 4,
    BitExtract = 5,
    WordSwap = 6,
    ByteSwap = 7,
    Clamp = 8,
    DefaultOnError = 9,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportTriggerType
{
    Always = 1,
    OnChange = 2,
    Deadband = 3,
    Interval = 4,
    QualityChange = 5,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QualityStatusType
{
    Good = 1,
    Uncertain = 2,
    Bad = 3,
    CommError = 4,
    InvalidData = 5,
}

/// <summary>
/// Edge 任务能力声明，用于表达执行端对任务类型、任务合同和回执行为的支持范围。
/// </summary>
public sealed record EdgeTaskCapabilityDto
{
    /// <summary>
    /// 支持的任务类型名称，推荐使用 EdgeTaskType 枚举名称。
    /// </summary>
    public string TaskType { get; init; } = string.Empty;

    /// <summary>
    /// 该任务能力遵循的任务合同版本。
    /// </summary>
    public string ContractVersion { get; init; } = EdgeNodeContractVersions.EdgeTaskV1;

    /// <summary>
    /// 执行端是否会在运行过程中回执进度。
    /// </summary>
    public bool SupportsProgress { get; init; } = true;

    /// <summary>
    /// 执行端是否支持取消该任务。
    /// </summary>
    public bool SupportsCancellation { get; init; }

    /// <summary>
    /// 非敏感任务能力扩展信息。
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// Edge 执行端与平台合同的兼容声明。
/// </summary>
public sealed record EdgeContractCompatibilityDto
{
    /// <summary>
    /// 合同名称，例如 edge-runtime、edge-capability、collection-config 或 edge-task。
    /// </summary>
    public string ContractName { get; init; } = string.Empty;

    /// <summary>
    /// 支持的合同版本。
    /// </summary>
    public string ContractVersion { get; init; } = string.Empty;

    /// <summary>
    /// 兼容的最低平台版本，未知时留空。
    /// </summary>
    public string MinPlatformVersion { get; init; } = string.Empty;

    /// <summary>
    /// 兼容的最高平台版本，未知或不限制时留空。
    /// </summary>
    public string MaxPlatformVersion { get; init; } = string.Empty;

    /// <summary>
    /// 该兼容项是否已废弃。
    /// </summary>
    public bool Deprecated { get; init; }

    /// <summary>
    /// 非敏感兼容性扩展信息。
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// Edge 能力快照，描述协议、点位类型、转换、任务和合同兼容范围。
/// </summary>
public sealed record EdgeCapabilityDto
{
    /// <summary>
    /// 能力快照合同版本。
    /// </summary>
    public string ContractVersion { get; init; } = EdgeNodeContractVersions.EdgeCapabilityV1;

    /// <summary>
    /// 平台侧 EdgeNode ID。
    /// </summary>
    public Guid EdgeNodeId { get; init; }

    /// <summary>
    /// 承载接入凭证的 Gateway 设备 ID。
    /// </summary>
    public Guid GatewayId { get; init; }

    /// <summary>
    /// 运行时类型，例如 gateway 或 collector。
    /// </summary>
    public string RuntimeType { get; init; } = string.Empty;

    /// <summary>
    /// 运行时实例名称。
    /// </summary>
    public string RuntimeName { get; init; } = string.Empty;

    /// <summary>
    /// 运行时软件版本。
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// 运行时实例标识。
    /// </summary>
    public string InstanceId { get; init; } = string.Empty;

    /// <summary>
    /// 执行端生成能力快照的 UTC 时间。
    /// </summary>
    public DateTime? ReportedAt { get; init; }

    /// <summary>
    /// 平台最后持久化能力快照的 UTC 时间。
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// 执行端上报的原始协议标识，用于保留自定义协议族名称。
    /// </summary>
    public IReadOnlyList<string> Protocols { get; init; } = [];

    /// <summary>
    /// 归一化后的协议枚举能力，用于与采集模板进行静态匹配。
    /// </summary>
    public IReadOnlyList<CollectionProtocolType> SupportedProtocols { get; init; } = [];

    /// <summary>
    /// 支持的点位源类型，例如 coil、holding-register、opcua-node。
    /// </summary>
    public IReadOnlyList<string> SupportedPointTypes { get; init; } = [];

    /// <summary>
    /// 支持的值转换能力。
    /// </summary>
    public IReadOnlyList<CollectionTransformType> SupportedTransforms { get; init; } = [];

    /// <summary>
    /// 支持的上报触发能力。
    /// </summary>
    public IReadOnlyList<ReportTriggerType> SupportedReportTriggers { get; init; } = [];

    /// <summary>
    /// 支持的平台能力标识。
    /// </summary>
    public IReadOnlyList<string> Features { get; init; } = [];

    /// <summary>
    /// 支持接收的任务能力标识，保留字符串形式以兼容未来任务类型。
    /// </summary>
    public IReadOnlyList<string> Tasks { get; init; } = [];

    /// <summary>
    /// 任务能力结构化声明。
    /// </summary>
    public IReadOnlyList<EdgeTaskCapabilityDto> TaskCapabilities { get; init; } = [];

    /// <summary>
    /// 执行端可兼容的合同版本。
    /// </summary>
    public IReadOnlyList<EdgeContractCompatibilityDto> CompatibleContracts { get; init; } = [];

    /// <summary>
    /// 非敏感能力扩展元数据。
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

public sealed record CollectionTaskDto
{
    public Guid Id { get; init; }
    public string TaskKey { get; init; } = string.Empty;
    public CollectionProtocolType Protocol { get; init; } = CollectionProtocolType.Unknown;
    public int Version { get; init; }
    public Guid EdgeNodeId { get; init; }
    public CollectionConnectionDto Connection { get; init; } = new();
    public IReadOnlyList<CollectionDeviceDto> Devices { get; init; } = [];
    public ReportPolicyDto ReportPolicy { get; init; } = new();
}

public sealed record CollectionConnectionDto
{
    public string ConnectionKey { get; init; } = string.Empty;
    public string ConnectionName { get; init; } = string.Empty;
    public CollectionProtocolType Protocol { get; init; } = CollectionProtocolType.Unknown;
    public string Transport { get; init; } = string.Empty;
    public string? Host { get; init; }
    public int? Port { get; init; }
    public string? SerialPort { get; init; }
    public int TimeoutMs { get; init; } = 3000;
    public int RetryCount { get; init; } = 3;
    public JsonElement? ProtocolOptions { get; init; }
}

public sealed record CollectionDeviceDto
{
    public string DeviceKey { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
    public string? ExternalKey { get; init; }
    public JsonElement? ProtocolOptions { get; init; }
    public IReadOnlyList<CollectionPointDto> Points { get; init; } = [];
}

public sealed record CollectionPointDto
{
    public string PointKey { get; init; } = string.Empty;
    public string PointName { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string RawValueType { get; init; } = string.Empty;
    public int Length { get; init; }
    public PollingPolicyDto Polling { get; init; } = new();
    public IReadOnlyList<ValueTransformDto> Transforms { get; init; } = [];
    public PlatformMappingDto Mapping { get; init; } = new();
    public JsonElement? ProtocolOptions { get; init; }
}

public sealed record PollingPolicyDto
{
    public int ReadPeriodMs { get; init; } = 5000;
    public string? Group { get; init; }
}

public sealed record ValueTransformDto
{
    public CollectionTransformType TransformType { get; init; } = CollectionTransformType.Scale;
    public int Order { get; init; }
    public JsonElement? Parameters { get; init; }
}

public sealed record PlatformMappingDto
{
    public CollectionTargetType TargetType { get; init; } = CollectionTargetType.Telemetry;
    public string TargetName { get; init; } = string.Empty;
    public CollectionValueType ValueType { get; init; } = CollectionValueType.Double;
    public string? DisplayName { get; init; }
    public string? Unit { get; init; }
    public string? Group { get; init; }
}

public sealed record ReportPolicyDto
{
    public ReportTriggerType DefaultTrigger { get; init; } = ReportTriggerType.OnChange;
    public double? Deadband { get; init; }
    public bool IncludeQuality { get; init; } = true;
    public bool IncludeTimestamp { get; init; } = true;
}

public sealed record TaskPreviewRequestDto
{
    public CollectionProtocolType Protocol { get; init; } = CollectionProtocolType.Unknown;
    public CollectionConnectionDto Connection { get; init; } = new();
    public CollectionDeviceDto Device { get; init; } = new();
    public CollectionPointDto Point { get; init; } = new();
}

public sealed record TaskPreviewResponseDto
{
    public bool Success { get; init; }
    public object? RawValue { get; init; }
    public object? TransformedValue { get; init; }
    public CollectionValueType ValueType { get; init; } = CollectionValueType.Double;
    public QualityStatusType QualityStatus { get; init; } = QualityStatusType.Good;
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}
