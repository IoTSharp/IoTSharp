using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts
{
    /// <summary>
    /// 采集配置分配状态。枚举值发布后不得重排或复用。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EdgeCollectionAssignmentStatus
    {
        /// <summary>
        /// 平台已记录分配，但尚未成为当前生效版本。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 当前生效的采集配置分配。
        /// </summary>
        Active = 1,

        /// <summary>
        /// 已被后续配置版本取代。
        /// </summary>
        Superseded = 2,

        /// <summary>
        /// 已被平台撤销。
        /// </summary>
        Revoked = 3
    }

    public sealed record EdgeCollectionConfigurationDto
    {
        public string ContractVersion { get; init; } = EdgeNodeContractVersions.CollectionConfigV1;
        public Guid EdgeNodeId { get; init; }
        public int Version { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源标识。由 Product Collection Template 生成时为模板 ID。
        /// </summary>
        public string SourceId { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源版本。由 Product Collection Template 生成时为模板版本。
        /// </summary>
        public string SourceVersion { get; init; } = string.Empty;

        /// <summary>
        /// 非敏感来源扩展信息。
        /// </summary>
        public Dictionary<string, object> SourceMetadata { get; init; } = [];

        public IReadOnlyList<CollectionTaskDto> Tasks { get; init; } = [];
    }

    /// <summary>
    /// Edge/Gateway 执行端拉取当前目标采集配置后的结果。
    /// </summary>
    public sealed record EdgeCollectionConfigurationPullResultDto
    {
        /// <summary>
        /// 配置正文合同版本。
        /// </summary>
        public string ContractVersion { get; init; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 承载接入凭证和配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; init; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; init; }

        /// <summary>
        /// 平台侧采集配置版本记录 ID。
        /// </summary>
        public Guid? ConfigurationVersionId { get; init; }

        /// <summary>
        /// Gateway 维度递增的配置版本号。
        /// </summary>
        public int ConfigurationVersion { get; init; }

        /// <summary>
        /// 配置文档哈希，用于执行端与发布任务参数核对。
        /// </summary>
        public string ConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 平台处理本次拉取的 UTC 时间。
        /// </summary>
        public DateTime PulledAt { get; init; }

        /// <summary>
        /// 当前目标分配快照；没有正式分配记录时为空以兼容旧配置。
        /// </summary>
        public EdgeCollectionAssignmentDto? Assignment { get; init; }

        /// <summary>
        /// 执行端需要缓存和应用的 collection-config-v1 正文。
        /// </summary>
        public EdgeCollectionConfigurationDto Configuration { get; init; } = new();
    }

    /// <summary>
    /// Edge 采集配置当前版本、目标版本和最近发布结果快照，用于管理端列表和详情展示。
    /// </summary>
    public sealed record EdgeCollectionVersionStatusDto
    {
        /// <summary>
        /// 采集配置状态快照合同版本。
        /// </summary>
        public string ContractVersion { get; init; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 承载接入凭证和配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; init; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; init; }

        /// <summary>
        /// 当前目标分配记录 ID。
        /// </summary>
        public Guid? AssignmentId { get; init; }

        /// <summary>
        /// 当前目标配置版本记录 ID。
        /// </summary>
        public Guid? TargetConfigurationVersionId { get; init; }

        /// <summary>
        /// 平台期望执行端应用的目标配置版本。
        /// </summary>
        public int? TargetConfigurationVersion { get; init; }

        /// <summary>
        /// 目标配置哈希。
        /// </summary>
        public string TargetConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 目标配置包含的采集任务数量。
        /// </summary>
        public int? TargetTaskCount { get; init; }

        /// <summary>
        /// 目标配置来源类型。
        /// </summary>
        public string TargetSourceType { get; init; } = string.Empty;

        /// <summary>
        /// 目标配置来源标识。
        /// </summary>
        public string TargetSourceId { get; init; } = string.Empty;

        /// <summary>
        /// 目标配置来源版本。
        /// </summary>
        public string TargetSourceVersion { get; init; } = string.Empty;

        /// <summary>
        /// 平台分配目标配置的 UTC 时间。
        /// </summary>
        public DateTime? TargetAssignedAt { get; init; }

        /// <summary>
        /// 执行端最近一次拉取目标配置的 UTC 时间。
        /// </summary>
        public DateTime? LastPulledAt { get; init; }

        /// <summary>
        /// 执行端最近确认成功应用的当前配置版本。
        /// </summary>
        public int? CurrentConfigurationVersion { get; init; }

        /// <summary>
        /// 执行端最近确认成功应用的当前配置哈希。
        /// </summary>
        public string CurrentConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 执行端最近确认成功应用配置的 UTC 时间。
        /// </summary>
        public DateTime? CurrentAppliedAt { get; init; }

        /// <summary>
        /// 当前版本是否已经与目标版本一致。
        /// </summary>
        public bool IsTargetApplied { get; init; }

        /// <summary>
        /// 当前版本与目标版本是否存在差异。
        /// </summary>
        public bool HasDifference { get; init; }

        /// <summary>
        /// 目标版本减去当前版本；无法计算时为空。
        /// </summary>
        public int? VersionDelta { get; init; }

        /// <summary>
        /// 面向管理端展示的差异摘要。
        /// </summary>
        public string DifferenceSummary { get; init; } = string.Empty;

        /// <summary>
        /// 最近一次配置发布或拉取请求任务 ID。
        /// </summary>
        public Guid? LastPublishTaskId { get; init; }

        /// <summary>
        /// 最近一次配置发布或拉取请求任务状态。
        /// </summary>
        public EdgeTaskStatus? LastPublishStatus { get; init; }

        /// <summary>
        /// 最近一次配置发布或拉取请求任务消息。
        /// </summary>
        public string LastPublishMessage { get; init; } = string.Empty;

        /// <summary>
        /// 最近一次配置发布或拉取请求任务进度。
        /// </summary>
        public int? LastPublishProgress { get; init; }

        /// <summary>
        /// 最近一次配置发布或拉取请求回执时间。
        /// </summary>
        public DateTime? LastPublishAt { get; init; }
    }

    public sealed record EdgeCollectionConfigurationUpdateDto
    {
        public IReadOnlyList<CollectionTaskDto> Tasks { get; init; } = [];

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源标识。由 Product Collection Template 生成时为模板 ID。
        /// </summary>
        public string SourceId { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源版本。由 Product Collection Template 生成时为模板版本。
        /// </summary>
        public string SourceVersion { get; init; } = string.Empty;

        /// <summary>
        /// 非敏感来源扩展信息。
        /// </summary>
        public Dictionary<string, object> SourceMetadata { get; init; } = [];
    }

    /// <summary>
    /// 平台侧采集配置版本快照，用于把 collection-config-v1 正文从目标分配关系中独立出来。
    /// </summary>
    public sealed record CollectionConfigurationVersionDto
    {
        /// <summary>
        /// 配置正文合同版本。
        /// </summary>
        public string ContractVersion { get; init; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 平台侧配置版本记录 ID。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 承载配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; init; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; init; }

        /// <summary>
        /// Gateway 维度递增的配置版本号。
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// 配置文档哈希，用于发布、拉取和回执核对。
        /// </summary>
        public string ConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 配置中包含的采集任务数量。
        /// </summary>
        public int TaskCount { get; init; }

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源标识。由 Product Collection Template 生成时为模板 ID。
        /// </summary>
        public string SourceId { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源版本。由 Product Collection Template 生成时为模板版本。
        /// </summary>
        public string SourceVersion { get; init; } = string.Empty;

        /// <summary>
        /// 非敏感来源扩展信息。
        /// </summary>
        public Dictionary<string, object> SourceMetadata { get; init; } = [];

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>
        /// 创建人显示名或账号标识。
        /// </summary>
        public string CreatedBy { get; init; } = string.Empty;

        /// <summary>
        /// 最近更新人显示名或账号标识。
        /// </summary>
        public string UpdatedBy { get; init; } = string.Empty;

        /// <summary>
        /// 完整 collection-config-v1 正文。列表接口可为空，详情接口返回。
        /// </summary>
        public EdgeCollectionConfigurationDto? Configuration { get; init; }
    }

    /// <summary>
    /// Edge 采集配置分配快照，用于回答某个配置版本当前被分配到哪个运行时目标。
    /// </summary>
    public sealed record EdgeCollectionAssignmentDto
    {
        /// <summary>
        /// 分配快照合同版本。
        /// </summary>
        public string ContractVersion { get; init; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 分配记录 ID。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 平台侧采集配置版本记录 ID。
        /// </summary>
        public Guid? CollectionConfigurationVersionId { get; init; }

        /// <summary>
        /// 分配目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; init; } = EdgeTaskTargetType.EdgeNode;

        /// <summary>
        /// 承载接入凭证和配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; init; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; init; }

        /// <summary>
        /// 标准寻址键，推荐格式为 deviceId:runtimeType 或 deviceId:runtimeType:instanceId。
        /// </summary>
        public string TargetKey { get; init; } = string.Empty;

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; init; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; init; } = string.Empty;

        /// <summary>
        /// 分配的采集配置版本号。
        /// </summary>
        public int ConfigurationVersion { get; init; }

        /// <summary>
        /// 配置文档哈希，用于执行端和平台核对是否为同一份配置。
        /// </summary>
        public string ConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 配置中包含的采集任务数量。
        /// </summary>
        public int TaskCount { get; init; }

        /// <summary>
        /// 当前分配状态。
        /// </summary>
        public EdgeCollectionAssignmentStatus Status { get; init; } = EdgeCollectionAssignmentStatus.Active;

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源标识。M3 模板实体落地前可为空。
        /// </summary>
        public string SourceId { get; init; } = string.Empty;

        /// <summary>
        /// 配置来源版本。M3 模板实体落地前默认使用配置版本号。
        /// </summary>
        public string SourceVersion { get; init; } = string.Empty;

        /// <summary>
        /// 平台创建分配的 UTC 时间。
        /// </summary>
        public DateTime AssignedAt { get; init; }

        /// <summary>
        /// 执行端最近一次拉取该配置的 UTC 时间。
        /// </summary>
        public DateTime? LastPulledAt { get; init; }

        /// <summary>
        /// 最近一次执行该配置分配的 EdgeTask ID。
        /// </summary>
        public Guid? LastExecutionTaskId { get; init; }

        /// <summary>
        /// 最近一次配置执行回执状态。
        /// </summary>
        public EdgeTaskStatus? LastExecutionStatus { get; init; }

        /// <summary>
        /// 最近一次配置执行回执消息。
        /// </summary>
        public string LastExecutionMessage { get; init; } = string.Empty;

        /// <summary>
        /// 最近一次配置执行进度，范围 0-100。
        /// </summary>
        public int? LastExecutionProgress { get; init; }

        /// <summary>
        /// 最近一次配置执行回执时间。
        /// </summary>
        public DateTime? LastExecutionAt { get; init; }

        /// <summary>
        /// 执行端已成功应用的配置版本号。
        /// </summary>
        public int? AppliedConfigurationVersion { get; init; }

        /// <summary>
        /// 执行端已成功应用的配置哈希。
        /// </summary>
        public string AppliedConfigurationHash { get; init; } = string.Empty;

        /// <summary>
        /// 执行端成功应用配置的 UTC 时间。
        /// </summary>
        public DateTime? AppliedAt { get; init; }

        /// <summary>
        /// 分配被撤销的 UTC 时间。
        /// </summary>
        public DateTime? RevokedAt { get; init; }

        /// <summary>
        /// 记录创建时间。
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// 记录更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>
        /// 创建人显示名或账号标识。
        /// </summary>
        public string CreatedBy { get; init; } = string.Empty;

        /// <summary>
        /// 最近更新人显示名或账号标识。
        /// </summary>
        public string UpdatedBy { get; init; } = string.Empty;

        /// <summary>
        /// 非敏感分配扩展信息。
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = [];
    }
}
