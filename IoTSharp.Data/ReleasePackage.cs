using IoTSharp.Contracts;
using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// ReleasePackage 最小软件包模型，保存平台上传的软件包元数据和 BlobStorage 位置。
    /// </summary>
    public class ReleasePackage : IJustMy
    {
        /// <summary>
        /// 软件包 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 软件包元数据合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.ReleasePackageV1;

        /// <summary>
        /// 包类型。
        /// </summary>
        public ReleasePackageType PackageType { get; set; } = ReleasePackageType.Software;

        /// <summary>
        /// 包业务键，例如 iotedge-gateway。
        /// </summary>
        public string PackageKey { get; set; }

        /// <summary>
        /// 包显示名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 包版本号。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 目标运行时类型，例如 gateway、collector 或 edge-node。
        /// </summary>
        public string TargetRuntimeType { get; set; }

        /// <summary>
        /// 目标运行时版本约束；为空表示由执行端自行判断兼容性。
        /// </summary>
        public string TargetRuntimeVersion { get; set; }

        /// <summary>
        /// 原始文件名。
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件内容类型。
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件字节数。
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件 SHA256 校验和，使用大写十六进制。
        /// </summary>
        public string Sha256 { get; set; }

        /// <summary>
        /// BlobStorage 中的包文件路径。
        /// </summary>
        public string BlobPath { get; set; }

        /// <summary>
        /// 下载令牌；仅通过软件更新任务参数下发给执行端。
        /// </summary>
        public string DownloadToken { get; set; }

        /// <summary>
        /// 非敏感包元数据 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建人显示名或账号。
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 最近更新人显示名或账号。
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 是否删除。软件包文件和历史默认保留，不做物理删除。
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
    }
}
