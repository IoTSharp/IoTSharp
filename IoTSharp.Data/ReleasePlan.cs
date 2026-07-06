using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Data
{
    /// <summary>
    /// Release Center 发布计划，承载软件、配置、OTA 的发布范围、灰度批次和确认策略。
    /// </summary>
    public class ReleasePlan : IJustMy
    {
        /// <summary>
        /// 发布计划 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 发布计划名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 发布计划描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 发布计划类型。
        /// </summary>
        public ReleasePlanType PlanType { get; set; } = ReleasePlanType.SoftwareUpdate;

        /// <summary>
        /// 当前发布计划状态。
        /// </summary>
        public ReleasePlanStatus Status { get; set; } = ReleasePlanStatus.Draft;

        /// <summary>
        /// 发布包 ID。
        /// </summary>
        public Guid? PackageId { get; set; }

        /// <summary>
        /// 发布包导航属性。
        /// </summary>
        public ReleasePackage Package { get; set; }

        /// <summary>
        /// 回滚包 ID。
        /// </summary>
        public Guid? RollbackPackageId { get; set; }

        /// <summary>
        /// 回滚包导航属性。
        /// </summary>
        public ReleasePackage RollbackPackage { get; set; }

        /// <summary>
        /// 发布确认策略。
        /// </summary>
        public ReleaseConfirmationPolicy ConfirmationPolicy { get; set; } = ReleaseConfirmationPolicy.ManualBetweenBatches;

        /// <summary>
        /// 灰度批次大小；小于等于 0 表示所有目标归入第一批。
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 是否允许失败后继续下发后续批次。
        /// </summary>
        public bool ContinueOnFailure { get; set; }

        /// <summary>
        /// 当前批次号。
        /// </summary>
        public int CurrentBatchNo { get; set; }

        /// <summary>
        /// 总任务数。
        /// </summary>
        public int TotalTaskCount { get; set; }

        /// <summary>
        /// 待下发任务数。
        /// </summary>
        public int PendingTaskCount { get; set; }

        /// <summary>
        /// 运行中任务数。
        /// </summary>
        public int RunningTaskCount { get; set; }

        /// <summary>
        /// 成功任务数。
        /// </summary>
        public int SucceededTaskCount { get; set; }

        /// <summary>
        /// 失败、超时或取消任务数。
        /// </summary>
        public int FailedTaskCount { get; set; }

        /// <summary>
        /// 非敏感计划元数据 JSON。
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
        /// 开始时间。
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 完成时间。
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 创建人显示名或账号。
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 最近更新人显示名或账号。
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 是否删除。发布历史默认保留，不做物理删除。
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
        /// 发布计划下的目标任务。
        /// </summary>
        public ICollection<ReleaseTask> Tasks { get; set; } = [];
    }
}
