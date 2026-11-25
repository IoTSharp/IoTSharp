using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos;

/// <summary>当前租户在所选范围内的消息总线和设备在线趋势。</summary>
public sealed record DashboardTrendDto(
    string Range,
    int BucketMinutes,
    string State,
    IReadOnlyList<DashboardTrendPointDto> Points);

/// <summary>单个 UTC 时间桶的计数和最后一次在线设备快照，空值表示该项缺少采样。</summary>
public sealed record DashboardTrendPointDto(
    DateTime StartUtc,
    long? Received,
    long? Processed,
    long? Telemetry,
    long? Attributes,
    int? OnlineDevices,
    bool Partial);
