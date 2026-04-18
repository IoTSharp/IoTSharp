using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace IoTSharp.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum CollectionTargetType
{
    Telemetry = 1,
    Attribute = 2,
    AlarmInput = 3,
    CommandFeedback = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum ReportTriggerType
{
    Always = 1,
    OnChange = 2,
    Deadband = 3,
    Interval = 4,
    QualityChange = 5,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum QualityStatusType
{
    Good = 1,
    Uncertain = 2,
    Bad = 3,
    CommError = 4,
    InvalidData = 5,
}

public sealed record EdgeCapabilityDto
{
    public string RuntimeType { get; init; } = string.Empty;
    public string RuntimeName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public IReadOnlyList<CollectionProtocolType> SupportedProtocols { get; init; } = [];
    public IReadOnlyList<string> SupportedPointTypes { get; init; } = [];
    public IReadOnlyList<CollectionTransformType> SupportedTransforms { get; init; } = [];
    public IReadOnlyList<ReportTriggerType> SupportedReportTriggers { get; init; } = [];
    public JsonElement? Metadata { get; init; }
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