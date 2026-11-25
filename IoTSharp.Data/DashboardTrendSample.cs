using System;

namespace IoTSharp.Data;

/// <summary>按租户保存已观测的分钟计数或在线设备快照。</summary>
public sealed class DashboardTrendSample
{
    public Guid TenantId { get; set; }
    public DateTime StartUtc { get; set; }
    public Guid CollectorId { get; set; }
    public long? Received { get; set; }
    public long? Processed { get; set; }
    public long? Telemetry { get; set; }
    public long? Attributes { get; set; }
    public int? OnlineDevices { get; set; }
    public DateTime? SnapshotUtc { get; set; }
}
