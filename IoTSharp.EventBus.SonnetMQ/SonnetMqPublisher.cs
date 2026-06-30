using System.Text;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Newtonsoft.Json;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqPublisher : IPublisher
{
    private static readonly IReadOnlyDictionary<string, string> JsonHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["content-type"] = "application/json"
    };

    private readonly SndbMqClient _client;

    public SonnetMqPublisher(SndbMqClient client)
    {
        _client = client;
    }

    public async Task<EventBusMetrics> GetMetrics()
    {
        var metrics = new EventBusMetrics();
        foreach (var subscription in SonnetMqEventBusTopics.Subscriptions)
        {
            var stats = await _client.GetStatsAsync(subscription.Topic);
            metrics.PublishedSucceeded += (int)Math.Min(int.MaxValue, stats.MessageCount);
            metrics.ReceivedSucceeded += (int)Math.Min(int.MaxValue, stats.ConsumerOffsets.Values.DefaultIfEmpty(0).Max());
            metrics.Subscribers += stats.ConsumerOffsets.Count;
        }

        return metrics;
    }

    public Task PublishCreateDevice(Guid devid)
        => PublishAsync(SonnetMqEventBusTopics.CreateDevice, devid);

    public Task PublishDeleteDevice(Guid devid)
        => PublishAsync(SonnetMqEventBusTopics.DeleteDevice, devid);

    public Task PublishAttributeData(PlayloadData msg)
        => PublishAsync(SonnetMqEventBusTopics.AttributeData, msg);

    public Task PublishTelemetryData(PlayloadData msg)
        => PublishAsync(SonnetMqEventBusTopics.TelemetryData, msg);

    public Task PublishConnect(Guid devid, ConnectStatus devicestatus)
        => PublishAsync(SonnetMqEventBusTopics.Connect, new DeviceConnectStatus(devid, devicestatus));

    public Task PublishActive(Guid devid, ActivityStatus activity)
        => PublishAsync(SonnetMqEventBusTopics.Active, new DeviceActivityStatus(devid, activity));

    public Task PublishDeviceAlarm(CreateAlarmDto alarmDto)
        => PublishAsync(SonnetMqEventBusTopics.Alarm, alarmDto);

    private Task PublishAsync<T>(string topic, T message)
    {
        byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        return _client.PublishAsync(topic, payload, JsonHeaders);
    }
}
