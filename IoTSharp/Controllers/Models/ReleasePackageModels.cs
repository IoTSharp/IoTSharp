using IoTSharp.Contracts;
using Microsoft.AspNetCore.Http;
using System;

namespace IoTSharp.Controllers.Models
{
    /// <summary>
    /// ReleasePackage 查询条件。
    /// </summary>
    public class ReleasePackageQueryDto : QueryDto
    {
        /// <summary>
        /// 包类型。
        /// </summary>
        public ReleasePackageType? PackageType { get; set; }

        /// <summary>
        /// 目标运行时类型。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 包版本号。
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// ReleasePackage 上传表单。
    /// </summary>
    public class ReleasePackageUploadRequestDto
    {
        /// <summary>
        /// 包类型；M5 起可保存采集器软件包、配置包、设备脚本和固件包元数据。
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
        /// 非敏感包元数据 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 软件包文件。
        /// </summary>
        public IFormFile File { get; set; }
    }
}
