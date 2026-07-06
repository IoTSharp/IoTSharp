using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    /// <summary>
    /// 采集配置版本快照，保存平台生成的 collection-config-v1 正文和来源信息。
    /// </summary>
    public class CollectionConfigurationVersion : IJustMy
    {
        /// <summary>
        /// 配置版本记录 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 配置正文合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.CollectionConfigV1;

        /// <summary>
        /// 承载配置拉取通道的 Gateway 设备 ID。
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 平台侧 EdgeNode ID。执行端未注册前可以为空。
        /// </summary>
        public Guid? EdgeNodeId { get; set; }

        /// <summary>
        /// Gateway 维度递增的配置版本号。
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 配置文档哈希，用于发布、拉取和回执核对。
        /// </summary>
        public string ConfigurationHash { get; set; }

        /// <summary>
        /// 配置中包含的采集任务数量。
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// 配置来源类型，例如 InlineCollectionConfig、ProductCollectionTemplate。
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 配置来源标识。由 Product Collection Template 生成时为模板 ID。
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// 配置来源版本。由 Product Collection Template 生成时为模板版本。
        /// </summary>
        public string SourceVersion { get; set; }

        /// <summary>
        /// 非敏感来源扩展信息 JSON。
        /// </summary>
        public string SourceMetadata { get; set; }

        /// <summary>
        /// 完整 collection-config-v1 配置正文 JSON。
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间。
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
        /// 是否删除。配置版本历史默认保留，不做物理删除。
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

        /// <summary>
        /// 引用该配置版本的目标分配记录。
        /// </summary>
        public ICollection<EdgeCollectionAssignment> Assignments { get; set; } = [];
    }
}
