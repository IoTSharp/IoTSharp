using System.Text;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqPublisher : IPublisher
{
    private static readonly IReadOnlyDictionary<string, string> JsonHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["content-type"] = "application/json"
    };

    private readonly SndbMqClient _client;
    private readonly SonnetMqActiveEventCoalescer _activeCoalescer;
    private readonly DashboardTrendCounters? _trend;

    public SonnetMqPublisher(SndbMqClient client, SonnetMqActiveEventCoalescer activeCoalescer, DashboardTrendCounters? trend = null)
    {
        _client = client;
        _activeCoalescer = activeCoalescer;
        _trend = trend;
    }

    public async Task<EventBusMetrics> GetMetrics()
    {
        var metrics = new EventBusMetrics();
        foreach (var subscription in SonnetMqEventBusTopics.Subscriptions)
        {
            var stats = await _client.GetStatsAsync(subscription.Topic);
            long received = stats.ConsumerOffsets.Values.DefaultIfEmpty(0).Max();
            metrics.PublishedSucceeded += (int)Math.Min(int.MaxValue, stats.NextOffset);
            metrics.ReceivedSucceeded += (int)Math.Min(int.MaxValue, received);
            metrics.PendingMessages += (int)Math.Min(int.MaxValue, Math.Max(0, stats.NextOffset - received));
            metrics.Subscribers += stats.ConsumerOffsets.Count;
        }

        return metrics;
    }

    public Task PublishCreateDevice(Guid devid)
        => PublishAsync(SonnetMqEventBusTopics.CreateDevice, devid, devid);

    public Task PublishDeleteDevice(Guid devid)
        => PublishAsync(SonnetMqEventBusTopics.DeleteDevice, devid, devid);

    public Task PublishAttributeData(PlayloadData msg)
        => PublishAsync(SonnetMqEventBusTopics.AttributeData, msg, msg.DeviceId, DashboardTrendCounters.ReportKind.Attribute);

    public Task PublishTelemetryData(PlayloadData msg)
        => PublishAsync(SonnetMqEventBusTopics.TelemetryData, msg, msg.DeviceId, DashboardTrendCounters.ReportKind.Telemetry);

    public Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        => PublishAsync(SonnetMqEventBusTopics.Connect, new DeviceConnectStatus(devid, devicestatus), devid);

    public async Task PublishActive(Guid devid, ActivityStatus activity)
    {
        var status = await _activeCoalescer.RecordAsync(devid, activity);
        if (status is not null)
        {
            await PublishAsync(SonnetMqEventBusTopics.Active, status, devid);
        }
    }

    public Task PublishDeviceAlarm(CreateAlarmDto alarmDto)
        => PublishAsync(SonnetMqEventBusTopics.Alarm, alarmDto, DashboardTrendCounters.AlarmDeviceId(alarmDto));

    private async Task PublishAsync<T>(string topic, T message, Guid deviceId,
        DashboardTrendCounters.ReportKind report = DashboardTrendCounters.ReportKind.Other)
    {
        byte[] payload = Encoding.UTF8.GetBytes(JsonObjectSerializer.Serialize(message));
        await _client.PublishAsync(topic, payload, JsonHeaders);
        _trend?.RecordReceived(deviceId, report);
    }
}
