using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Data
{
    /// <summary>
    /// Release Center 发布任务，表示一个发布计划面向一个目标的一次执行。
    /// </summary>
    public class ReleaseTask : IJustMy
    {
        /// <summary>
        /// 发布任务 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属发布计划 ID。
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// 所属发布计划。
        /// </summary>
        public ReleasePlan Plan { get; set; }

        /// <summary>
        /// 发布包 ID。
        /// </summary>
        public Guid? PackageId { get; set; }

        /// <summary>
        /// 发布包。
        /// </summary>
        public ReleasePackage Package { get; set; }

        /// <summary>
        /// 发布目标类型。
        /// </summary>
        public ReleaseTargetType TargetType { get; set; }

        /// <summary>
        /// 发布目标 ID。
        /// </summary>
        public Guid? TargetId { get; set; }

        /// <summary>
        /// 承载任务投递通道的 Gateway 设备 ID。
        /// </summary>
        public Guid? GatewayId { get; set; }

        /// <summary>
        /// 平台侧 EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; set; }

        /// <summary>
        /// 标准寻址键，推荐格式为 deviceId:runtimeType 或 deviceId:runtimeType:instanceId。
        /// </summary>
        public string TargetKey { get; set; }

        /// <summary>
        /// 目标运行时类型。
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 灰度批次号，从 1 开始。
        /// </summary>
        public int BatchNo { get; set; }

        /// <summary>
        /// 当前发布任务状态。
        /// </summary>
        public ReleaseTaskStatus Status { get; set; } = ReleaseTaskStatus.Pending;

        /// <summary>
        /// 是否为回滚任务。
        /// </summary>
        public bool IsRollback { get; set; }

        /// <summary>
        /// 绑定的正式 EdgeTask ID。
        /// </summary>
        public Guid? EdgeTaskId { get; set; }

        /// <summary>
        /// 绑定的正式 EdgeTask。
        /// </summary>
        public EdgeTask EdgeTask { get; set; }

        /// <summary>
        /// 最近状态说明。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 最近执行进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 非敏感任务元数据 JSON。
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
        /// 下发时间。
        /// </summary>
        public DateTime? DispatchedAt { get; set; }

        /// <summary>
        /// 完成时间。
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 最近回执时间。
        /// </summary>
        public DateTime? LastReceiptAt { get; set; }

        /// <summary>
        /// 是否删除。发布任务历史默认保留，不做物理删除。
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
        /// 发布任务回执历史。
        /// </summary>
        public ICollection<ReleaseReceipt> Receipts { get; set; } = [];
    }
}
