using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using System;

namespace IoTSharp.Dtos
{
    /// <summary>
    /// EdgeNode 列表查询条件；云边注册、心跳、能力和状态 DTO 统一来自 IoTSharp.Contracts。
    /// </summary>
    public class EdgeNodeQueryDto : QueryDto
    {
        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 运行状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 健康状态过滤。
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// 设备活动状态过滤。
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// 运行时版本。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 运行平台描述。
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 排序字段。
        /// </summary>
        public string Sorter { get; set; }

        /// <summary>
        /// 排序方向，支持 asc 或 desc。
        /// </summary>
        public string Sort { get; set; }
    }

    /// <summary>
    /// Edge 采集配置分配查询条件。
    /// </summary>
    public class EdgeCollectionAssignmentQueryDto : QueryDto
    {
        /// <summary>
        /// Gateway 设备 ID。
        /// </summary>
        public Guid? GatewayId { get; set; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; set; }

        /// <summary>
        /// 分配目标类型名称。
        /// </summary>
        public string TargetType { get; set; }

        /// <summary>
        /// 分配状态。
        /// </summary>
        public EdgeCollectionAssignmentStatus? Status { get; set; }

        /// <summary>
        /// 采集配置版本号。
        /// </summary>
        public int? ConfigurationVersion { get; set; }

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 标准寻址键。
        /// </summary>
        public string TargetKey { get; set; }

        /// <summary>
        /// 配置来源类型。
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 排序字段。
        /// </summary>
        public string Sorter { get; set; }

        /// <summary>
        /// 排序方向，支持 asc 或 desc。
        /// </summary>
        public string Sort { get; set; }
    }
}
