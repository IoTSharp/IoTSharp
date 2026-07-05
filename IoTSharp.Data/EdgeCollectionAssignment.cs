using IoTSharp.Contracts;
using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// Edge 采集配置分配记录，承载配置版本到 EdgeNode、Gateway 或设备范围的目标关系。
    /// </summary>
    public class EdgeCollectionAssignment : IJustMy
    {
        /// <summary>
        /// 分配记录 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 采集配置合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 分配目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; set; } = EdgeTaskTargetType.EdgeNode;

        /// <summary>
        /// 承载配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 平台侧 EdgeNode ID。执行端未注册前可以为空。
        /// </summary>
        public Guid? EdgeNodeId { get; set; }

        /// <summary>
        /// 标准寻址键，推荐格式为 deviceId:runtimeType 或 deviceId:runtimeType:instanceId。
        /// </summary>
        public string TargetKey { get; set; }

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 分配的采集配置版本号。
        /// </summary>
        public int ConfigurationVersion { get; set; }

        /// <summary>
        /// 配置文档哈希，用于执行端和平台核对是否为同一份配置。
        /// </summary>
        public string ConfigurationHash { get; set; }

        /// <summary>
        /// 配置中包含的采集任务数量。
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// 当前分配状态。
        /// </summary>
        public EdgeCollectionAssignmentStatus Status { get; set; } = EdgeCollectionAssignmentStatus.Active;

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 配置来源标识。M3 模板实体落地前可以为空。
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// 配置来源版本。M3 模板实体落地前默认使用配置版本号。
        /// </summary>
        public string SourceVersion { get; set; }

        /// <summary>
        /// 非敏感分配扩展信息 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 平台创建分配的 UTC 时间。
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 执行端最近一次拉取该配置的 UTC 时间。
        /// </summary>
        public DateTime? LastPulledAt { get; set; }

        /// <summary>
        /// 分配被撤销的 UTC 时间。
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// 记录创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 记录更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建人显示名或账号标识。
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 最近更新人显示名或账号标识。
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 是否删除。配置分配历史默认保留，不做物理删除。
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 租户 ID。
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 租户导航属性。
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户 ID。
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// 客户导航属性。
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// 承载配置拉取通道的 Gateway 设备。
        /// </summary>
        public Device Gateway { get; set; }
    }
}
