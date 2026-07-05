using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts
{
    /// <summary>
    /// Edge 任务目标类型。枚举值发布后不得重排或复用。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EdgeTaskTargetType
    {
        /// <summary>
        /// 面向受管 EdgeNode 运行时。
        /// </summary>
        EdgeNode = 0,

        /// <summary>
        /// 面向同时承担 Gateway 职责的运行时。
        /// </summary>
        GatewayRuntime = 1,

        /// <summary>
        /// 面向 EdgeNode 下的一组设备。
        /// </summary>
        DeviceScope = 3
    }

    /// <summary>
    /// Edge 任务类型。新增任务类型只能追加。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EdgeTaskType
    {
        /// <summary>
        /// 推送配置。
        /// </summary>
        ConfigPush = 0,

        /// <summary>
        /// 要求执行端主动拉取配置。
        /// </summary>
        ConfigPullRequest = 1,

        /// <summary>
        /// 下载软件、固件或诊断包。
        /// </summary>
        PackageDownload = 2,

        /// <summary>
        /// 应用已下载的软件、固件或配置包。
        /// </summary>
        PackageApply = 3,

        /// <summary>
        /// 重启运行时。
        /// </summary>
        RestartRuntime = 4,

        /// <summary>
        /// 执行健康探测。
        /// </summary>
        HealthProbe = 5
    }

    /// <summary>
    /// Edge 任务状态。状态语义一旦发布不得改变。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EdgeTaskStatus
    {
        /// <summary>
        /// 平台已创建任务，尚未投递。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 平台已把任务放入执行端可拉取通道。
        /// </summary>
        Sent = 1,

        /// <summary>
        /// 执行端已接收并确认受理。
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// 执行端正在执行。
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
        /// 平台或执行端取消任务。
        /// </summary>
        Cancelled = 7
    }

    /// <summary>
    /// Edge 任务状态机定义。
    /// </summary>
    public static class EdgeTaskStateMachine
    {
        /// <summary>
        /// 已发布状态机允许的单向状态流转。
        /// </summary>
        public static IReadOnlyDictionary<EdgeTaskStatus, EdgeTaskStatus[]> AllowedTransitions { get; } =
            new Dictionary<EdgeTaskStatus, EdgeTaskStatus[]>
            {
                [EdgeTaskStatus.Pending] = [EdgeTaskStatus.Sent],
                [EdgeTaskStatus.Sent] = [EdgeTaskStatus.Accepted, EdgeTaskStatus.TimedOut],
                [EdgeTaskStatus.Accepted] = [EdgeTaskStatus.Running, EdgeTaskStatus.Cancelled],
                [EdgeTaskStatus.Running] = [EdgeTaskStatus.Succeeded, EdgeTaskStatus.Failed, EdgeTaskStatus.TimedOut, EdgeTaskStatus.Cancelled],
                [EdgeTaskStatus.Succeeded] = [],
                [EdgeTaskStatus.Failed] = [],
                [EdgeTaskStatus.TimedOut] = [],
                [EdgeTaskStatus.Cancelled] = []
            };

        /// <summary>
        /// 已发布终态集合。
        /// </summary>
        public static EdgeTaskStatus[] TerminalStates { get; } =
        [
            EdgeTaskStatus.Succeeded,
            EdgeTaskStatus.Failed,
            EdgeTaskStatus.TimedOut,
            EdgeTaskStatus.Cancelled
        ];

        /// <summary>
        /// 判断状态流转是否符合合同定义；重复上报同一状态视为幂等。
        /// </summary>
        /// <param name="current">当前已记录状态。</param>
        /// <param name="next">准备上报的新状态。</param>
        /// <returns>允许流转时返回 true。</returns>
        public static bool IsTransitionAllowed(EdgeTaskStatus current, EdgeTaskStatus next)
        {
            if (current == next)
            {
                return true;
            }

            return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
        }
    }

    /// <summary>
    /// Edge 任务寻址信息。
    /// </summary>
    public class EdgeTaskAddressDto
    {
        /// <summary>
        /// 目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; set; }

        /// <summary>
        /// 承载任务的 Gateway 或 Device ID。
        /// </summary>
        public Guid? DeviceId { get; set; }

        /// <summary>
        /// 接入令牌。仅用于兼容短链路寻址，不建议作为长期主键。
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 标准寻址键，推荐格式为 deviceId:runtimeType 或 deviceId:runtimeType:instanceId。
        /// </summary>
        public string TargetKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// 平台下发给 Edge 运行时的任务请求。
    /// </summary>
    public class EdgeTaskRequestDto
    {
        /// <summary>
        /// 合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeTaskV1;

        /// <summary>
        /// 全局任务标识。
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 任务类型。
        /// </summary>
        public EdgeTaskType TaskType { get; set; }

        /// <summary>
        /// 目标寻址信息。
        /// </summary>
        public EdgeTaskAddressDto Address { get; set; } = new();

        /// <summary>
        /// 任务创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 任务过期时间。
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 任务参数。
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = [];

        /// <summary>
        /// 非敏感任务元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// 平台侧 Edge 任务状态快照，用于管理端查询和跨仓验收。
    /// </summary>
    public class EdgeTaskDto
    {
        /// <summary>
        /// 合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeTaskV1;

        /// <summary>
        /// 全局任务标识。
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 任务类型。
        /// </summary>
        public EdgeTaskType TaskType { get; set; }

        /// <summary>
        /// 目标寻址信息。
        /// </summary>
        public EdgeTaskAddressDto Address { get; set; } = new();

        /// <summary>
        /// 当前任务状态。
        /// </summary>
        public EdgeTaskStatus Status { get; set; }

        /// <summary>
        /// 最近一次状态说明。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 最近一次进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 任务创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

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
        /// 任务参数。
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = [];

        /// <summary>
        /// 非敏感任务元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// Edge 运行时上报的任务回执。
    /// </summary>
    public class EdgeTaskReceiptDto
    {
        /// <summary>
        /// 合同版本。
        /// </summary>
        public string ContractVersion { get; set; } = EdgeNodeContractVersions.EdgeTaskV1;

        /// <summary>
        /// 对应任务 ID。
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 目标类型。
        /// </summary>
        public EdgeTaskTargetType TargetType { get; set; }

        /// <summary>
        /// 标准寻址键。
        /// </summary>
        public string TargetKey { get; set; } = string.Empty;

        /// <summary>
        /// 运行时类型。
        /// </summary>
        public string RuntimeType { get; set; } = string.Empty;

        /// <summary>
        /// 运行时实例标识。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 回执状态。
        /// </summary>
        public EdgeTaskStatus Status { get; set; }

        /// <summary>
        /// 人类可读状态说明。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 执行端上报回执的 UTC 时间。
        /// </summary>
        public DateTime ReportedAt { get; set; }

        /// <summary>
        /// 任务进度，范围 0-100。
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// 任务结果载荷。
        /// </summary>
        public Dictionary<string, object> Result { get; set; } = [];

        /// <summary>
        /// 非敏感回执元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}
