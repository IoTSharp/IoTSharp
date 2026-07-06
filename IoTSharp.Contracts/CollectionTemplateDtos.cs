using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts;

/// <summary>
/// 采集模板状态。枚举值发布后不得重排或复用。
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionTemplateStatus
{
    /// <summary>
    /// 草稿，可继续编辑，不能直接作为发布依据。
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已校验通过，可生成边缘运行时配置。
    /// </summary>
    Active = 1,

    /// <summary>
    /// 已归档，仅保留历史引用。
    /// </summary>
    Archived = 2,
}

/// <summary>
/// 点位访问语义。
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionPointAccess
{
    /// <summary>
    /// 只读采集点。
    /// </summary>
    Read = 1,

    /// <summary>
    /// 只写点。
    /// </summary>
    Write = 2,

    /// <summary>
    /// 可读写点。
    /// </summary>
    ReadWrite = 3,

    /// <summary>
    /// 命令型点。
    /// </summary>
    Command = 4,

    /// <summary>
    /// 配置型点。
    /// </summary>
    Config = 5,
}

/// <summary>
/// Product 下的一等采集模板。
/// </summary>
public sealed record CollectionTemplateDto
{
    /// <summary>
    /// 模板 ID。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 所属 Product ID。
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Product 内稳定模板键。
    /// </summary>
    public string TemplateKey { get; init; } = string.Empty;

    /// <summary>
    /// 模板名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 模板描述。
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// semantic-core 模型批次或导入来源标识。
    /// </summary>
    public string SemanticModelId { get; init; } = string.Empty;

    /// <summary>
    /// 模板版本号。M3 阶段使用模板自身版本，不替代后续配置版本模型。
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// 模板状态。
    /// </summary>
    public CollectionTemplateStatus Status { get; init; } = CollectionTemplateStatus.Draft;

    /// <summary>
    /// 是否允许生成运行时配置。
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// 协议模板。
    /// </summary>
    public ProtocolTemplateDto Protocol { get; init; } = new();

    /// <summary>
    /// 连接模板集合。
    /// </summary>
    public IReadOnlyList<ConnectionTemplateDto> Connections { get; init; } = [];

    /// <summary>
    /// 点位模板集合。
    /// </summary>
    public IReadOnlyList<PointTemplateDto> Points { get; init; } = [];

    /// <summary>
    /// 默认上报策略。
    /// </summary>
    public ReportPolicyDto ReportPolicy { get; init; } = new();

    /// <summary>
    /// 非敏感扩展信息。
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// 创建人显示名或账号。
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// 最近更新人显示名或账号。
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>
/// 新增或修改采集模板的请求。
/// </summary>
public sealed record CollectionTemplateUpsertDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string TemplateKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SemanticModelId { get; init; } = string.Empty;
    public int Version { get; init; }
    public CollectionTemplateStatus Status { get; init; } = CollectionTemplateStatus.Draft;
    public bool Enabled { get; init; } = true;
    public ProtocolTemplateDto Protocol { get; init; } = new();
    public IReadOnlyList<ConnectionTemplateDto> Connections { get; init; } = [];
    public IReadOnlyList<PointTemplateDto> Points { get; init; } = [];
    public ReportPolicyDto ReportPolicy { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// 协议模板，描述协议族和协议级非敏感参数。
/// </summary>
public sealed record ProtocolTemplateDto
{
    public Guid Id { get; init; }
    public CollectionProtocolType Protocol { get; init; } = CollectionProtocolType.Unknown;
    public string ProtocolKind { get; init; } = string.Empty;
    public JsonElement? Parameters { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// 连接模板，描述地址、串口、认证类型、超时和重试。
/// </summary>
public sealed record ConnectionTemplateDto
{
    public Guid Id { get; init; }
    public string ConnectionKey { get; init; } = string.Empty;
    public string ConnectionName { get; init; } = string.Empty;
    public string Transport { get; init; } = string.Empty;
    public string EndpointRef { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public int? Port { get; init; }
    public string SerialPort { get; init; } = string.Empty;
    public string AuthType { get; init; } = string.Empty;
    public int TimeoutMs { get; init; } = 3000;
    public int RetryCount { get; init; } = 3;
    public JsonElement? ProtocolOptions { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// 点位模板，保存点位语义、协议绑定、采样、转换和平台映射。
/// </summary>
public sealed record PointTemplateDto
{
    public Guid Id { get; init; }
    public string ConnectionKey { get; init; } = string.Empty;
    public string PointKey { get; init; } = string.Empty;
    public string SemanticId { get; init; } = string.Empty;
    public string BindingId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string FieldPath { get; init; } = string.Empty;
    public string RawValueType { get; init; } = string.Empty;
    public CollectionValueType ValueType { get; init; } = CollectionValueType.Double;
    public CollectionPointAccess Access { get; init; } = CollectionPointAccess.Read;
    public int Length { get; init; } = 1;
    public string Quantity { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
    public JsonElement? ProtocolOptions { get; init; }
    public JsonElement? QualityPolicy { get; init; }
    public SamplingPolicyTemplateDto SamplingPolicy { get; init; } = new();
    public MappingPolicyTemplateDto Mapping { get; init; } = new();
    public IReadOnlyList<TransformTemplateDto> Transforms { get; init; } = [];
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// 值转换模板。
/// </summary>
public sealed record TransformTemplateDto
{
    public Guid Id { get; init; }
    public CollectionTransformType TransformType { get; init; } = CollectionTransformType.Scale;
    public int Order { get; init; }
    public JsonElement? Parameters { get; init; }
}

/// <summary>
/// 采样策略模板。
/// </summary>
public sealed record SamplingPolicyTemplateDto
{
    public Guid Id { get; init; }
    public int ReadPeriodMs { get; init; } = 5000;
    public int? TimeoutMs { get; init; }
    public ReportTriggerType Trigger { get; init; } = ReportTriggerType.OnChange;
    public double? Deadband { get; init; }
    public bool ReportOnQualityChange { get; init; } = true;
    public bool Subscription { get; init; }
    public string AggregateHint { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
}

/// <summary>
/// 平台映射策略模板。
/// </summary>
public sealed record MappingPolicyTemplateDto
{
    public Guid Id { get; init; }
    public CollectionTargetType TargetType { get; init; } = CollectionTargetType.Telemetry;
    public string TargetName { get; init; } = string.Empty;
    public CollectionValueType ValueType { get; init; } = CollectionValueType.Double;
    public string DisplayName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
}

/// <summary>
/// 采集模板诊断级别。
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionTemplateDiagnosticSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
}

/// <summary>
/// 采集模板校验诊断。
/// </summary>
public sealed record CollectionTemplateDiagnosticDto
{
    public CollectionTemplateDiagnosticSeverity Severity { get; init; } = CollectionTemplateDiagnosticSeverity.Error;
    public string Code { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// 采集模板校验结果。
/// </summary>
public sealed record CollectionTemplateValidationResultDto
{
    public bool Success { get; init; }
    public IReadOnlyList<CollectionTemplateDiagnosticDto> Diagnostics { get; init; } = [];
}

/// <summary>
/// 从模板生成运行时配置的请求。
/// </summary>
public sealed record CollectionTemplateRuntimeConfigRequestDto
{
    public Guid EdgeNodeId { get; init; }
    public int? Version { get; init; }
}

/// <summary>
/// 从 Product 采集模板创建配置发布任务的请求。
/// </summary>
public sealed record CollectionTemplateConfigurationPublishRequestDto
{
    /// <summary>
    /// 目标 EdgeNode ID。当前实现与承载接入凭证的 Gateway 设备 ID 一一对应。
    /// </summary>
    public Guid EdgeNodeId { get; init; }

    /// <summary>
    /// 可选任务 ID。为空时由平台生成，非空时用于调用方关联外部操作记录。
    /// </summary>
    public Guid? TaskId { get; init; }

    /// <summary>
    /// 任务目标类型，默认面向 Gateway 运行时。
    /// </summary>
    public EdgeTaskTargetType TargetType { get; init; } = EdgeTaskTargetType.GatewayRuntime;

    /// <summary>
    /// 目标运行时类型。为空时使用 EdgeNode 最近上报的运行时类型。
    /// </summary>
    public string RuntimeType { get; init; } = string.Empty;

    /// <summary>
    /// 目标运行时实例。为空时使用 EdgeNode 最近上报的实例 ID。
    /// </summary>
    public string InstanceId { get; init; } = string.Empty;

    /// <summary>
    /// 发布任务过期时间。为空时由平台使用默认过期窗口。
    /// </summary>
    public DateTime? ExpireAt { get; init; }

    /// <summary>
    /// 非敏感任务元数据。
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}

/// <summary>
/// 从 Product 采集模板创建配置发布任务后的结果。
/// </summary>
public sealed record CollectionTemplateConfigurationPublishResultDto
{
    /// <summary>
    /// 平台保存的采集配置版本快照。
    /// </summary>
    public CollectionConfigurationVersionDto ConfigurationVersion { get; init; } = new();

    /// <summary>
    /// 当前生效的配置目标分配。
    /// </summary>
    public EdgeCollectionAssignmentDto Assignment { get; init; } = new();

    /// <summary>
    /// 创建后的 EdgeTask 发布请求，执行端通过 edge-task-v1 拉取。
    /// </summary>
    public EdgeTaskRequestDto Task { get; init; } = new();
}

/// <summary>
/// 采集模板预览请求。
/// </summary>
public sealed record CollectionTemplatePreviewRequestDto
{
    public string PointKey { get; init; } = string.Empty;
    public JsonElement? RawValue { get; init; }
}
