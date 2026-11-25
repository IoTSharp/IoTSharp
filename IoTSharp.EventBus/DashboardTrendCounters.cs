using IoTSharp.Contracts;
using IoTSharp.Data;

namespace IoTSharp.EventBus;

/// <summary>在进程内累计事件总线的分钟计数，采集线程负责按设备归属汇总持久化。</summary>
public sealed class DashboardTrendCounters
{
    private readonly object _gate = new();
    private readonly TimeProvider _clock;
    private readonly DateTime _firstFullMinute;
    private DateTime _currentMinute;
    private Dictionary<Guid, Counts> _current = new();
    private Dictionary<Guid, Counts> _previous = new();
    private DateTime _lastTakenMinute;

    public DashboardTrendCounters(TimeProvider? clock = null)
    {
        _clock = clock ?? TimeProvider.System;
        _currentMinute = MinuteStart(_clock.GetUtcNow().UtcDateTime);
        _firstFullMinute = _currentMinute.AddMinutes(1);
        _lastTakenMinute = _currentMinute;
    }

    /// <summary>发布成功后增加接收计数，按显式数据主题累计遥测和属性上报。</summary>
    public void RecordReceived(Guid deviceId, ReportKind report = ReportKind.Other)
        => Record(deviceId, new Counts(1, 0, report == ReportKind.Telemetry ? 1 : 0, report == ReportKind.Attribute ? 1 : 0));

    /// <summary>设备消息的订阅处理器正常返回后增加处理计数。</summary>
    public void RecordProcessed(Guid deviceId) => Record(deviceId, new Counts(0, 1, 0, 0));

    /// <summary>批量遥测的订阅处理器正常返回后，一条上报增加一次处理计数。</summary>
    public void RecordProcessed(IReadOnlyCollection<PlayloadData> messages)
    {
        lock (_gate)
        {
            var minute = MinuteStart(_clock.GetUtcNow().UtcDateTime);
            Advance(minute);
            if (minute != _currentMinute || minute < _firstFullMinute)
                return;

            var delta = new Counts(0, 1, 0, 0);
            foreach (var message in messages)
                Add(message.DeviceId, delta);
        }
    }

    /// <summary>取走最近完整观测的一分钟以及该分钟的设备计数。未观测的分钟返回空。</summary>
    public (DateTime StartUtc, IReadOnlyDictionary<Guid, Counts> Devices)? TakeCompletedMinute()
    {
        lock (_gate)
        {
            var minute = MinuteStart(_clock.GetUtcNow().UtcDateTime);
            Advance(minute);
            var completed = minute.AddMinutes(-1);
            if (minute != _currentMinute || completed < _firstFullMinute || _lastTakenMinute >= completed)
                return null;

            // 进程暂停跨过多个分钟时，保留未知区间，避免补写零值。
            if (completed > _lastTakenMinute.AddMinutes(1))
            {
                _lastTakenMinute = completed;
                _previous = new();
                return null;
            }

            _lastTakenMinute = completed;
            var counts = _previous;
            _previous = new();
            return (completed, counts);
        }
    }

    /// <summary>按分钟向下对齐 UTC 时间。</summary>
    public static DateTime MinuteStart(DateTime utc)
        => new(utc.Ticks - utc.Ticks % TimeSpan.TicksPerMinute, DateTimeKind.Utc);

    /// <summary>只有明确指向设备 GUID 的告警可以归入租户趋势。</summary>
    public static Guid AlarmDeviceId(CreateAlarmDto alarm)
        => alarm.OriginatorType == OriginatorType.Device && Guid.TryParse(alarm.OriginatorName, out var deviceId)
            ? deviceId : Guid.Empty;

    private void Record(Guid deviceId, Counts delta)
    {
        lock (_gate)
        {
            var minute = MinuteStart(_clock.GetUtcNow().UtcDateTime);
            Advance(minute);
            if (minute == _currentMinute && minute >= _firstFullMinute)
                Add(deviceId, delta);
        }
    }

    private void Advance(DateTime minute)
    {
        if (minute <= _currentMinute)
            return;

        _previous = minute == _currentMinute.AddMinutes(1) ? _current : new();
        _current = new();
        _currentMinute = minute;
    }

    private void Add(Guid deviceId, Counts delta)
    {
        if (deviceId != Guid.Empty)
            _current[deviceId] = _current.GetValueOrDefault(deviceId) + delta;
    }

    public enum ReportKind
    {
        Other,
        Telemetry,
        Attribute
    }

    public readonly record struct Counts(long Received, long Processed, long Telemetry, long Attributes)
    {
        public static Counts operator +(Counts left, Counts right)
            => new(left.Received + right.Received, left.Processed + right.Processed,
                left.Telemetry + right.Telemetry, left.Attributes + right.Attributes);
    }
}
