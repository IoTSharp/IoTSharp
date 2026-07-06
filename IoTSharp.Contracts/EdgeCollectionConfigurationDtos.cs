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
