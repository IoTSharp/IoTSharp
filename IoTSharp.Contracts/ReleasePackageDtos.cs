using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts
{
    /// <summary>
    /// ReleasePackage 包类型。新增类型只能追加，不能重排既有值。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleasePackageType
    {
        /// <summary>
        /// 面向 EdgeNode 或 Gateway 运行时的软件更新包。
        /// </summary>
        Software = 0
    }

    /// <summary>
    /// ReleasePackage 元数据 DTO，用于平台 API 和跨仓软件更新任务参数。
    /// </summary>
    public class ReleasePackageDto
    {
        /// <summary>
        /// 合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.ReleasePackageV1;

        /// <summary>
        /// 软件包 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 包类型。
        /// </summary>
        public ReleasePackageType PackageType { get; set; } = ReleasePackageType.Software;

        /// <summary>
        /// 包业务键，例如 iotedge-gateway。
        /// </summary>
        public string PackageKey { get; set; } = string.Empty;

        /// <summary>
        /// 包显示名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 包版本号。
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 目标运行时类型，例如 gateway、collector 或 edge-node。
        /// </summary>
        public string TargetRuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 目标运行时版本约束；为空表示由执行端自行判断兼容性。
        /// </summary>
        public string TargetRuntimeVersion { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件内容类型。
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 文件字节数。
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件 SHA256 校验和，使用大写十六进制。
        /// </summary>
        public string Sha256 { get; set; } = string.Empty;

        /// <summary>
        /// 非敏感包元数据。
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = [];

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 创建人显示名或账号。
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 最近更新人显示名或账号。
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// ReleasePackage 发布到 EdgeTask 的请求。
    /// </summary>
    public class ReleasePackagePublishRequestDto
    {
        /// <summary>
        /// 目标 EdgeNode 或 Gateway 设备 ID。
        /// </summary>
        public Guid EdgeNodeId { get; set; }

        /// <summary>
        /// 任务目标类型；为空时平台根据运行时类型选择默认值。
        /// </summary>
        public EdgeTaskTargetType? TargetType { get; set; }

        /// <summary>
        /// 目标运行时类型；为空时使用包的 TargetRuntimeType。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 目标运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 可选任务 ID；为空时由平台生成。
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// 任务过期时间；为空时使用平台默认软件更新任务生命周期。
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 追加到任务的非敏感元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// ReleasePackage 发布结果。
    /// </summary>
    public class ReleasePackagePublishResultDto
    {
        /// <summary>
        /// 已发布的软件包元数据。
        /// </summary>
        public ReleasePackageDto Package { get; set; } = new();

        /// <summary>
        /// 创建出的软件更新 EdgeTask。
        /// </summary>
        public EdgeTaskRequestDto Task { get; set; } = new();
    }
}
