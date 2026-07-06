using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos
{
    /// <summary>
    /// Edge 任务失败重试请求。
    /// </summary>
    public class EdgeTaskRetryRequestDto
    {
        /// <summary>
        /// 可选的新任务 ID；为空时由平台生成。
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// 重试原因，写入新任务元数据和审计日志。
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 新任务过期时间；为空时沿用原任务生命周期长度。
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 追加到新任务的非敏感元数据。
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    /// <summary>
    /// Edge 任务失败重试结果。
    /// </summary>
    public class EdgeTaskRetryResultDto
    {
        /// <summary>
        /// 原失败任务当前态。
        /// </summary>
        public IoTSharp.Contracts.EdgeTaskDto OriginalTask { get; set; }

        /// <summary>
        /// 新创建的重试任务请求。
        /// </summary>
        public IoTSharp.Contracts.EdgeTaskRequestDto RetryTask { get; set; }
    }

    /// <summary>
    /// EdgeTask 审计日志查询结果。
    /// </summary>
    public class EdgeTaskAuditLogDto
    {
        /// <summary>
        /// 审计记录 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务 ID。
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 动作名称。
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 动作数据 JSON。
        /// </summary>
        public string ActionData { get; set; }

        /// <summary>
        /// 动作结果摘要。
        /// </summary>
        public string ActionResult { get; set; }

        /// <summary>
        /// 操作者显示名或运行时来源。
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 审计时间。
        /// </summary>
        public DateTime ActiveDateTime { get; set; }
    }

    /// <summary>
    /// Edge 任务时间线节点，仅用于管理端查询展示；云边合同 DTO 统一来自 IoTSharp.Contracts。
    /// </summary>
    public class EdgeTaskTimelineNodeDto
    {
        /// <summary>
        /// 事件类别，例如 request、dispatch 或 receipt。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 事件发生时的任务状态名称。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 执行端或平台记录的状态说明。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 事件发生时间。
        /// </summary>
        public DateTime At { get; set; }

        /// <summary>
        /// 事件原始载荷，用于排障展开查看。
        /// </summary>
        public string Payload { get; set; }
    }

    /// <summary>
    /// Edge 任务管理端时间线查询结果。
    /// </summary>
    public class EdgeTaskTimelineDto
    {
        /// <summary>
        /// 承载任务通道的 Gateway 设备 ID。
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Gateway 设备名称。
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 任务 ID。
        /// </summary>
        public Guid TaskId { get; set; }

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
        public string CurrentStatus { get; set; }

        /// <summary>
        /// 最近更新时间。
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// 时间线事件集合。
        /// </summary>
        public List<EdgeTaskTimelineNodeDto> Events { get; set; }
    }
}
