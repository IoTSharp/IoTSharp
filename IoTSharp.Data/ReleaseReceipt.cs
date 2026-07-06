using IoTSharp.Contracts;
using System;

namespace IoTSharp.Data
{
    /// <summary>
    /// Release Center 发布回执，记录执行端回报到发布任务维度后的生命周期事件。
    /// </summary>
    public class ReleaseReceipt : IJustMy
    {
        /// <summary>
        /// 发布回执 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 发布计划 ID。
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// 发布计划。
        /// </summary>
        public ReleasePlan Plan { get; set; }

        /// <summary>
        /// 发布任务 ID。
        /// </summary>
        public Guid ReleaseTaskId { get; set; }

        /// <summary>
        /// 发布任务。
        /// </summary>
        public ReleaseTask ReleaseTask { get; set; }

        /// <summary>
        /// 绑定的正式 EdgeTask ID。
        /// </summary>
        public Guid? EdgeTaskId { get; set; }

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
        /// 标准寻址键。
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
        /// 回执状态。
        /// </summary>
        public ReleaseTaskStatus Status { get; set; }

        /// <summary>
        /// 状态说明。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 进度，范围 0-100。
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
        /// 原始 EdgeTask 回执 JSON。
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// 执行端上报时间。
        /// </summary>
        public DateTime ReportedAt { get; set; }

        /// <summary>
        /// 平台接收时间。
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否删除。发布回执历史默认保留，不做物理删除。
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
