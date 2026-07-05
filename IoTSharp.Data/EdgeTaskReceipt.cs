using IoTSharp.Contracts;
using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// Edge 任务回执历史，记录执行端对任务生命周期每一步的正式上报。
    /// </summary>
    public class EdgeTaskReceipt : IJustMy
    {
        /// <summary>
        /// 回执记录 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 对应的 Edge 任务 ID。
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 回执合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeTaskV1;

        /// <summary>
        /// 任务目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; set; }

        /// <summary>
        /// 承载任务投递和回执通道的 Gateway 设备 ID。
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
        /// 回执状态。
        /// </summary>
        public EdgeTaskStatus Status { get; set; }

        /// <summary>
        /// 执行端上报的状态说明。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 执行进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 回执结果 JSON。
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 非敏感回执元数据 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 原始回执 JSON，用于跨仓排障和兼容旧历史视图。
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// 执行端上报回执的 UTC 时间。
        /// </summary>
        public DateTime ReportedAt { get; set; }

        /// <summary>
        /// 平台接收回执的 UTC 时间。
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否删除。回执历史默认保留，不做物理删除。
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
        /// 对应的 Edge 任务。
        /// </summary>
        public EdgeTask Task { get; set; }

        /// <summary>
        /// 承载任务通道的 Gateway 设备。
        /// </summary>
        public Device Gateway { get; set; }
    }
}
