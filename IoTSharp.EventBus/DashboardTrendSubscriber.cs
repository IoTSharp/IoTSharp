using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTSharp.EventBus;

/// <summary>在订阅处理器正常返回后记录趋势，保留各消息总线原有的重试和确认行为。</summary>
public abstract class DashboardTrendSubscriber(
    ILogger<EventBusSubscriber> logger,
    IServiceScopeFactory scopeFactory,
    IStorage storage,
    IEasyCachingProviderFactory factory,
    EventBusOption options,
    DashboardTrendCounters? trend)
    : EventBusSubscriber(logger, scopeFactory, storage, factory, options), ISubscriber
{
    public new async Task StoreAttributeData(PlayloadData message)
    {
        await base.StoreAttributeData(message);
        trend?.RecordProcessed(message.DeviceId);
    }

    public new async Task StoreTelemetryData(PlayloadData message)
    {
        await base.StoreTelemetryData(message);
        trend?.RecordProcessed(message.DeviceId);
    }

    public new async Task StoreTelemetryDataBatch(IReadOnlyCollection<PlayloadData> messages)
    {
        await base.StoreTelemetryDataBatch(messages);
        trend?.RecordProcessed(messages);
    }

    public new async Task OccurredAlarm(CreateAlarmDto alarm)
    {
        await base.OccurredAlarm(alarm);
        trend?.RecordProcessed(DashboardTrendCounters.AlarmDeviceId(alarm));
    }

    public new async Task CreateDevice(Guid deviceId)
    {
        await base.CreateDevice(deviceId);
        trend?.RecordProcessed(deviceId);
    }

    public new async Task DeleteDevice(Guid deviceId)
    {
        await base.DeleteDevice(deviceId);
        trend?.RecordProcessed(deviceId);
    }

    public new async Task Connect(Guid deviceId, ConnectStatus status)
    {
        await base.Connect(deviceId, status);
        trend?.RecordProcessed(deviceId);
    }

    public new async Task Active(Guid deviceId, ActivityStatus status, DateTime? eventTimeUtc = null)
    {
        await base.Active(deviceId, status, eventTimeUtc);
        trend?.RecordProcessed(deviceId);
    }
}
