using System.Text;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqEventBusWorker : BackgroundService
{
    private readonly SndbMqClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SonnetMqEventBusWorker> _logger;
    private readonly SonnetMqEventBusOptions _options;

    public SonnetMqEventBusWorker(
        SndbMqClient client,
        IServiceScopeFactory scopeFactory,
        IOptions<SonnetMqEventBusOptions> options,
        ILogger<SonnetMqEventBusWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool processed = false;
            foreach (var subscription in SonnetMqEventBusTopics.Subscriptions)
            {
                processed |= await PullAndDispatchAsync(subscription, stoppingToken);
            }

            if (!processed)
                await Task.Delay(Math.Max(1, _options.IdleDelayMilliseconds), stoppingToken);
        }
    }

    private async Task<bool> PullAndDispatchAsync(SonnetMqSubscription subscription, CancellationToken cancellationToken)
    {
        var messages = await _client.PullAsync(
            subscription.Topic,
            _options.ConsumerGroup,
            Math.Max(1, _options.PullBatchSize),
            cancellationToken);
        if (messages.Count == 0)
            return false;

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var subscriber = scope.ServiceProvider.GetRequiredService<ISubscriber>();
                await DispatchAsync(subscriber, subscription.Kind, message);
                await _client.AckAsync(subscription.Topic, _options.ConsumerGroup, message.Offset, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SonnetMQ event dispatch failed. topic={Topic}, offset={Offset}",
                    message.Topic,
                    message.Offset);
                return true;
            }
        }

        return true;
    }

    private static Task DispatchAsync(ISubscriber subscriber, string kind, SndbMqMessage message)
        => kind switch
        {
            SonnetMqEventKinds.AttributeData => subscriber.StoreAttributeData(ReadJson<PlayloadData>(message)),
            SonnetMqEventKinds.TelemetryData => subscriber.StoreTelemetryData(ReadJson<PlayloadData>(message)),
            SonnetMqEventKinds.Alarm => subscriber.OccurredAlarm(ReadJson<CreateAlarmDto>(message)),
            SonnetMqEventKinds.CreateDevice => subscriber.CreateDevice(ReadJson<Guid>(message)),
            SonnetMqEventKinds.DeleteDevice => subscriber.DeleteDevice(ReadJson<Guid>(message)),
            SonnetMqEventKinds.Connect => DispatchConnectAsync(subscriber, message),
            SonnetMqEventKinds.Active => DispatchActiveAsync(subscriber, message),
            _ => Task.CompletedTask
        };

    private static Task DispatchConnectAsync(ISubscriber subscriber, SndbMqMessage message)
    {
        var status = ReadJson<DeviceConnectStatus>(message);
        return subscriber.Connect(status.DeviceId, status.ConnectStatus);
    }

    private static Task DispatchActiveAsync(ISubscriber subscriber, SndbMqMessage message)
    {
        var status = ReadJson<DeviceActivityStatus>(message);
        return subscriber.Active(status.DeviceId, status.Activity);
    }

    private static T ReadJson<T>(SndbMqMessage message)
    {
        string json = Encoding.UTF8.GetString(message.Payload);
        return JsonConvert.DeserializeObject<T>(json)
            ?? throw new InvalidDataException($"SonnetMQ payload cannot be deserialized as {typeof(T).Name}.");
    }
}
