using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos
{
    public enum EdgeTaskTargetType
    {
        EdgeNode = 0,
        GatewayRuntime = 1,
        PixiuRuntime = 2,
        DeviceScope = 3
    }

    public enum EdgeTaskType
    {
        ConfigPush = 0,
        ConfigPullRequest = 1,
        PackageDownload = 2,
        PackageApply = 3,
        RestartRuntime = 4,
        HealthProbe = 5
    }

    public enum EdgeTaskStatus
    {
        Pending = 0,
        Sent = 1,
        Accepted = 2,
        Running = 3,
        Succeeded = 4,
        Failed = 5,
        TimedOut = 6,
        Cancelled = 7
    }

    public class EdgeTaskAddressDto
    {
        public EdgeTaskTargetType TargetType { get; set; }
        public Guid? DeviceId { get; set; }
        public string AccessToken { get; set; }
        public string RuntimeType { get; set; }
        public string InstanceId { get; set; }
        public string TargetKey { get; set; }
    }

    public class EdgeTaskRequestDto
    {
        public string ContractVersion { get; set; }
        public Guid TaskId { get; set; }
        public EdgeTaskType TaskType { get; set; }
        public EdgeTaskAddressDto Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class EdgeTaskReceiptDto
    {
        public string ContractVersion { get; set; }
        public Guid TaskId { get; set; }
        public EdgeTaskTargetType TargetType { get; set; }
        public string TargetKey { get; set; }
        public string RuntimeType { get; set; }
        public string InstanceId { get; set; }
        public EdgeTaskStatus Status { get; set; }
        public string Message { get; set; }
        public DateTime ReportedAt { get; set; }
        public int? Progress { get; set; }
        public Dictionary<string, object> Result { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class EdgeTaskListItemDto
    {
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; }
        public Guid TaskId { get; set; }
        public string Category { get; set; }
        public string RuntimeType { get; set; }
        public string InstanceId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime At { get; set; }
        public string Payload { get; set; }
    }

    public class EdgeTaskTimelineNodeDto
    {
        public string Category { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime At { get; set; }
        public string Payload { get; set; }
    }

    public class EdgeTaskTimelineDto
    {
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; }
        public Guid TaskId { get; set; }
        public string RuntimeType { get; set; }
        public string InstanceId { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<EdgeTaskTimelineNodeDto> Events { get; set; }
    }
}