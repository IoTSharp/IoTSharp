using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts
{
    /// <summary>
    /// Release Center 发布计划类型。新增类型只能追加，不能重排既有值。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleasePlanType
    {
        /// <summary>
        /// EdgeNode、Gateway 或采集器运行时软件更新。
        /// </summary>
        SoftwareUpdate = 0,

        /// <summary>
        /// 配置版本发布。
        /// </summary>
        ConfigurationRollout = 1,

        /// <summary>
        /// 设备脚本 OTA。
        /// </summary>
        DeviceScriptOta = 2,

        /// <summary>
        /// 设备或运行时固件 OTA。
        /// </summary>
        FirmwareOta = 3
    }

    /// <summary>
    /// Release Center 发布目标类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleaseTargetType
    {
        /// <summary>
        /// 受管 EdgeNode 运行时。
        /// </summary>
        EdgeNode = 0,

        /// <summary>
        /// Gateway 运行时。
        /// </summary>
        Gateway = 1,

        /// <summary>
        /// 单个 Device。
        /// </summary>
        Device = 2,

        /// <summary>
        /// Asset 业务范围。
        /// </summary>
        AssetScope = 3,

        /// <summary>
        /// Device 范围集合。
        /// </summary>
        DeviceScope = 4
    }

    /// <summary>
    /// Release Center 发布计划状态。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleasePlanStatus
    {
        /// <summary>
        /// 已创建但尚未下发。
        /// </summary>
        Draft = 0,

        /// <summary>
        /// 等待人工确认后继续。
        /// </summary>
        WaitingConfirmation = 1,

        /// <summary>
        /// 已下发，存在进行中的任务。
        /// </summary>
        Running = 2,

        /// <summary>
        /// 已暂停，不会继续下发后续批次。
        /// </summary>
        Paused = 3,

        /// <summary>
        /// 所有发布任务成功完成。
        /// </summary>
        Succeeded = 4,

        /// <summary>
        /// 部分任务成功，部分任务失败、超时或取消。
        /// </summary>
        PartiallySucceeded = 5,

        /// <summary>
        /// 发布失败且没有成功任务。
        /// </summary>
        Failed = 6,

        /// <summary>
        /// 已取消。
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// 正在执行回滚。
        /// </summary>
        RollingBack = 8,

        /// <summary>
        /// 回滚完成。
        /// </summary>
        RolledBack = 9,

        /// <summary>
        /// 回滚失败。
        /// </summary>
        RollbackFailed = 10
    }

    /// <summary>
    /// Release Center 发布任务状态。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleaseTaskStatus
    {
        /// <summary>
        /// 已创建，尚未生成投递任务。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 已进入执行端可拉取通道。
        /// </summary>
        Sent = 1,

        /// <summary>
        /// 执行端已确认受理。
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// 执行中。
        /// </summary>
        Running = 3,

        /// <summary>
        /// 执行成功。
        /// </summary>
        Succeeded = 4,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Failed = 5,

        /// <summary>
        /// 执行超时。
        /// </summary>
        TimedOut = 6,

        /// <summary>
        /// 已取消。
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// 回滚成功。
        /// </summary>
        RolledBack = 8,

        /// <summary>
        /// 回滚失败。
        /// </summary>
        RollbackFailed = 9
    }

    /// <summary>
    /// 发布确认策略。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReleaseConfirmationPolicy
    {
        /// <summary>
        /// 不需要人工确认，开始后下发所有批次。
        /// </summary>
        None = 0,

        /// <summary>
        /// 开始前需要人工确认。
        /// </summary>
        ManualBeforeStart = 1,

        /// <summary>
        /// 每个灰度批次之间需要人工确认。
        /// </summary>
        ManualBetweenBatches = 2
    }

    /// <summary>
    /// 灰度发布策略。
    /// </summary>
    public class ReleaseRolloutStrategyDto
    {
        /// <summary>
        /// 每批目标数；小于等于 0 时表示所有目标归入第一批。
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 是否允许失败后继续下发后续批次。
        /// </summary>
        public bool ContinueOnFailure { get; set; }
    }

    /// <summary>
    /// 发布目标定义。
    /// </summary>
    public class ReleaseTargetDto
    {
        /// <summary>
        /// 目标类型。
        /// </summary>
        public ReleaseTargetType TargetType { get; set; } = ReleaseTargetType.EdgeNode;

        /// <summary>
        /// 目标 ID，EdgeNode 或 Gateway 第一版必须传入。
        /// </summary>
        public Guid TargetId { get; set; }

        /// <summary>
        /// 目标运行时类型；为空时平台按目标类型和包元数据推导。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 可选标准寻址键；为空时平台生成。
        /// </summary>
        public string TargetKey { get; set; } = string.Empty;

        /// <summary>
        /// 非敏感目标元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// 创建发布计划请求。
    /// </summary>
    public class ReleasePlanCreateRequestDto
    {
        /// <summary>
        /// 发布计划名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 发布计划描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 发布计划类型。
        /// </summary>
        public ReleasePlanType PlanType { get; set; } = ReleasePlanType.SoftwareUpdate;

        /// <summary>
        /// 发布包 ID。
        /// </summary>
        public Guid PackageId { get; set; }

        /// <summary>
        /// 回滚包 ID；执行回滚时可覆盖为新的包。
        /// </summary>
        public Guid? RollbackPackageId { get; set; }

        /// <summary>
        /// 发布确认策略。
        /// </summary>
        public ReleaseConfirmationPolicy ConfirmationPolicy { get; set; } = ReleaseConfirmationPolicy.ManualBetweenBatches;

        /// <summary>
        /// 灰度发布策略。
        /// </summary>
        public ReleaseRolloutStrategyDto Strategy { get; set; } = new();

        /// <summary>
        /// 创建后是否立即开始。
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// 发布目标列表。
        /// </summary>
        public List<ReleaseTargetDto> Targets { get; set; } = [];

        /// <summary>
        /// 非敏感计划元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// 发布计划操作请求。
    /// </summary>
    public class ReleasePlanActionRequestDto
    {
        /// <summary>
        /// 操作原因。
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 回滚使用的软件包 ID；为空时使用计划上的 RollbackPackageId。
        /// </summary>
        public Guid? RollbackPackageId { get; set; }

        /// <summary>
        /// 是否强制继续后续批次。
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// 操作附带的非敏感元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// 发布任务状态快照。
    /// </summary>
    public class ReleaseTaskDto
    {
        /// <summary>
        /// 发布任务 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 发布计划 ID。
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// 发布包 ID。
        /// </summary>
        public Guid? PackageId { get; set; }

        /// <summary>
        /// 目标类型。
        /// </summary>
        public ReleaseTargetType TargetType { get; set; }

        /// <summary>
        /// 目标 ID。
        /// </summary>
        public Guid? TargetId { get; set; }

        /// <summary>
        /// 承载任务通道的 Gateway ID。
        /// </summary>
        public Guid? GatewayId { get; set; }

        /// <summary>
        /// EdgeNode ID。
        /// </summary>
        public Guid? EdgeNodeId { get; set; }

        /// <summary>
        /// 标准寻址键。
        /// </summary>
        public string TargetKey { get; set; } = string.Empty;

        /// <summary>
        /// 目标运行时类型。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 灰度批次号，从 1 开始。
        /// </summary>
        public int BatchNo { get; set; }

        /// <summary>
        /// 当前发布任务状态。
        /// </summary>
        public ReleaseTaskStatus Status { get; set; }

        /// <summary>
        /// 是否为回滚任务。
        /// </summary>
        public bool IsRollback { get; set; }

        /// <summary>
        /// 绑定的 EdgeTask ID。
        /// </summary>
        public Guid? EdgeTaskId { get; set; }

        /// <summary>
        /// 最近状态说明。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 最近进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; }

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
        /// 非敏感任务元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// 发布回执快照。
    /// </summary>
    public class ReleaseReceiptDto
    {
        /// <summary>
        /// 发布回执 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 发布计划 ID。
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// 发布任务 ID。
        /// </summary>
        public Guid ReleaseTaskId { get; set; }

        /// <summary>
        /// 绑定的 EdgeTask ID。
        /// </summary>
        public Guid? EdgeTaskId { get; set; }

        /// <summary>
        /// 回执状态。
        /// </summary>
        public ReleaseTaskStatus Status { get; set; }

        /// <summary>
        /// 状态说明。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 回执结果。
        /// </summary>
        public Dictionary<string, object> Result { get; set; } = [];

        /// <summary>
        /// 非敏感回执元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];

        /// <summary>
        /// 执行端上报时间。
        /// </summary>
        public DateTime ReportedAt { get; set; }

        /// <summary>
        /// 平台接收时间。
        /// </summary>
        public DateTime ReceivedAt { get; set; }
    }

    /// <summary>
    /// 发布计划状态快照。
    /// </summary>
    public class ReleasePlanDto
    {
        /// <summary>
        /// 发布计划 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 发布计划名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 发布计划描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 发布计划类型。
        /// </summary>
        public ReleasePlanType PlanType { get; set; }

        /// <summary>
        /// 发布计划状态。
        /// </summary>
        public ReleasePlanStatus Status { get; set; }

        /// <summary>
        /// 发布包 ID。
        /// </summary>
        public Guid? PackageId { get; set; }

        /// <summary>
        /// 回滚包 ID。
        /// </summary>
        public Guid? RollbackPackageId { get; set; }

        /// <summary>
        /// 发布确认策略。
        /// </summary>
        public ReleaseConfirmationPolicy ConfirmationPolicy { get; set; }

        /// <summary>
        /// 每批目标数。
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 是否允许失败后继续。
        /// </summary>
        public bool ContinueOnFailure { get; set; }

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
        /// 失败任务数。
        /// </summary>
        public int FailedTaskCount { get; set; }

        /// <summary>
        /// 当前批次号。
        /// </summary>
        public int CurrentBatchNo { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; }

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
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 最近更新人显示名或账号。
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 非敏感计划元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];

        /// <summary>
        /// 计划关联的发布任务。
        /// </summary>
        public List<ReleaseTaskDto> Tasks { get; set; } = [];
    }

    /// <summary>
    /// 发布计划操作结果。
    /// </summary>
    public class ReleasePlanOperationResultDto
    {
        /// <summary>
        /// 发布计划快照。
        /// </summary>
        public ReleasePlanDto Plan { get; set; } = new();

        /// <summary>
        /// 本次操作创建或下发的 EdgeTask。
        /// </summary>
        public List<EdgeTaskRequestDto> EdgeTasks { get; set; } = [];
    }
}
