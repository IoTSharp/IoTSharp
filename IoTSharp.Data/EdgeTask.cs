using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Data
{
    /// <summary>
    /// Edge 任务主模型，承载平台向 EdgeNode、Gateway 运行时或设备范围下发的任务当前态。
    /// </summary>
    public class EdgeTask : IJustMy
    {
        /// <summary>
        /// 全局任务标识，对应云边合同中的 TaskId。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeTaskV1;

        /// <summary>
        /// 任务类型。
        /// </summary>
        public EdgeTaskType TaskType { get; set; }

        /// <summary>
        /// 任务目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; set; }

        /// <summary>
        /// 承载任务投递通道的 Gateway 设备 ID。
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
        /// 当前任务状态。
        /// </summary>
        public EdgeTaskStatus Status { get; set; } = EdgeTaskStatus.Pending;

        /// <summary>
        /// 最近一次状态说明。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 最近一次进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 任务参数 JSON。
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 非敏感任务元数据 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 原始任务请求 JSON，用于投递给执行端和兼容旧历史视图。
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// 最近一次回执 JSON。完整回执历史由后续 EdgeTaskReceipt 模型承载。
        /// </summary>
        public string LastReceiptPayload { get; set; }

        /// <summary>
        /// 任务创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 任务过期时间。
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 任务首次进入可拉取通道的时间。
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// 执行端确认受理时间。
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// 执行端开始执行时间。
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 任务进入终态的时间。
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 最近一次回执时间。
        /// </summary>
        public DateTime? LastReceiptAt { get; set; }

        /// <summary>
        /// 最近一次状态更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否删除。任务历史默认保留，不做物理删除。
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
        /// 承载任务通道的 Gateway 设备。
        /// </summary>
        public Device Gateway { get; set; }

        /// <summary>
        /// 执行端上报的正式回执历史。
        /// </summary>
        public ICollection<EdgeTaskReceipt> Receipts { get; set; } = [];
    }
}
