using System.Text;
using System.Collections.Concurrent;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SonnetDB.Data.Mq;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqEventBusWorker : BackgroundService
{
    private readonly SndbMqClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SonnetMqEventBusWorker> _logger;
    private readonly SonnetMqEventBusOptions _options;
    private readonly ConcurrentDictionary<string, int> _deliveryFailures = new(StringComparer.Ordinal);

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
        var diagnostics = SonnetMqProcessingLog.RunAsync(_logger, _options.ProcessingDiagnosticsSeconds, stoppingToken);
        var workers = SonnetMqEventBusTopics.Subscriptions
            .Select(subscription => RunSubscriptionWorkerAsync(subscription, stoppingToken))
            .ToArray();

        await Task.WhenAll(workers.Append(diagnostics));
    }

    private async Task RunSubscriptionWorkerAsync(SonnetMqSubscription subscription, CancellationToken stoppingToken)
    {
        int recoveryDelaySeconds = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool processed = await PullAndDispatchAsync(subscription, stoppingToken);
                recoveryDelaySeconds = 1;
                if (!processed)
                    await Task.Delay(Math.Max(1, _options.IdleDelayMilliseconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.Partial)
            {
                _logger.LogCritical(
                    "SonnetMQ subscription halted for reconciliation. Topic={Topic}, error={Error}.",
                    subscription.Topic, SonnetDbWriteRecovery.Partial);
                return;
            }
            catch (Exception ex) when (SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.Rejected)
            {
                _logger.LogWarning(
                    "SonnetMQ kept the current consumer position while storage recovers. Topic={Topic}, error={Error}, retryDelaySeconds={RetryDelaySeconds}.",
                    subscription.Topic, SonnetDbWriteRecovery.Required, recoveryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(recoveryDelaySeconds), stoppingToken);
                recoveryDelaySeconds = Math.Min(30, recoveryDelaySeconds * 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SonnetMQ event worker failed. Topic={Topic}. It will retry after a short delay.", subscription.Topic);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task<bool> PullAndDispatchAsync(SonnetMqSubscription subscription, CancellationToken cancellationToken)
    {
        IReadOnlyList<SndbMqMessage> messages;
        using (SonnetMqProcessingMetrics.Measure(subscription.Kind switch
        {
            SonnetMqEventKinds.TelemetryData => SonnetMqProcessingMetrics.TelemetryPull,
            SonnetMqEventKinds.AttributeData => SonnetMqProcessingMetrics.AttributePull,
            _ => null
        }))
        {
            messages = await _client.PullAsync(
                subscription.Topic,
                _options.ConsumerGroup,
                Math.Max(1, _options.PullBatchSize),
                cancellationToken);
        }
        if (messages.Count == 0)
            return false;

        if (subscription.Kind == SonnetMqEventKinds.TelemetryData)
            return await DispatchTelemetryBatchAsync(subscription, messages, cancellationToken);

        return await DispatchMessagesIndividuallyAsync(subscription, messages, cancellationToken);
    }

    private async Task<bool> DispatchMessagesIndividuallyAsync(
        SonnetMqSubscription subscription,
        IReadOnlyList<SndbMqMessage> messages,
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var subscriber = scope.ServiceProvider.GetRequiredService<ISubscriber>();
                using (SonnetMqProcessingMetrics.Measure(subscription.Kind switch
                {
                    SonnetMqEventKinds.TelemetryData => SonnetMqProcessingMetrics.TelemetrySingleDispatch,
                    SonnetMqEventKinds.AttributeData => SonnetMqProcessingMetrics.AttributeDispatch,
                    _ => null
                }))
                using (SonnetDbWriteRecovery.Enable())
                {
                    await DispatchAsync(subscriber, subscription.Kind, message);
                }
                await AckAsync(subscription, message.Offset, cancellationToken);
                ClearDeliveryFailure(message);
            }
            catch (Exception ex) when (SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.None)
            {
                bool skipped = await HandleDispatchFailureAsync(subscription, message, ex, cancellationToken);
                if (!skipped)
                    return true;
            }
        }

        return true;
    }

    private async Task<bool> DispatchTelemetryBatchAsync(
        SonnetMqSubscription subscription,
        IReadOnlyList<SndbMqMessage> messages,
        CancellationToken cancellationToken)
    {
        try
        {
            using var dispatch = SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryBatchDispatch);
            using var recovery = SonnetDbWriteRecovery.Enable();
            using var scope = _scopeFactory.CreateScope();
            var subscriber = scope.ServiceProvider.GetRequiredService<ISubscriber>();
            PlayloadData[] payloads;
            using (SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryBatchDecode))
            {
                payloads = messages.Select(ReadJson<PlayloadData>).ToArray();
            }

            await subscriber.StoreTelemetryDataBatch(payloads);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested
            && SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.None)
        {
            _logger.LogWarning(
                ex,
                "SonnetMQ telemetry batch dispatch failed. Falling back to per-message dispatch. topic={Topic}, firstOffset={FirstOffset}, lastOffset={LastOffset}, count={Count}",
                subscription.Topic,
                messages[0].Offset,
                messages[^1].Offset,
                messages.Count);
            try
            {
                return await DispatchMessagesIndividuallyAsync(subscription, messages, cancellationToken);
            }
            catch (Exception recoveryError) when (SonnetDbWriteRecovery.Classify(recoveryError) == SonnetDbWriteRecovery.Outcome.Rejected)
            {
                // 先前失败的批次可能已经写入一部分，逐条拒绝无法确认批次前缀。
                throw SonnetDbWriteRecovery.AfterEarlierWrites(recoveryError);
            }
        }

        // 批次全部处理成功后，SonnetMQ 一次确认到最后一条。确认失败时由下一轮拉取按服务端位点重试。
        await AckAsync(subscription, messages[^1].Offset, cancellationToken);
        foreach (var message in messages)
        {
            ClearDeliveryFailure(message);
        }

        return true;
    }

    private async Task<bool> HandleDispatchFailureAsync(
        SonnetMqSubscription subscription,
        SndbMqMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string key = DeliveryFailureKey(message);
        int attempts = _deliveryFailures.AddOrUpdate(key, 1, static (_, current) => current + 1);
        int maxAttempts = Math.Max(1, _options.MaxDeliveryAttempts);

        if (attempts < maxAttempts)
        {
            _logger.LogWarning(
                exception,
                "SonnetMQ event dispatch failed. It will be retried. topic={Topic}, offset={Offset}, attempt={Attempt}, maxAttempts={MaxAttempts}",
                message.Topic,
                message.Offset,
                attempts,
                maxAttempts);
            return false;
        }

        await PublishDeadLetterAsync(subscription, message, exception, attempts, cancellationToken);
        await AckAsync(subscription, message.Offset, cancellationToken);
        ClearDeliveryFailure(message);
        _logger.LogError(
            exception,
            "SonnetMQ event dispatch failed permanently and was moved to dead letter. topic={Topic}, offset={Offset}, attempts={Attempts}",
            message.Topic,
            message.Offset,
            attempts);
        return true;
    }

    /// <summary>确认已完成的位点并记录遥测或属性的确认耗时。</summary>
    private async Task AckAsync(SonnetMqSubscription subscription, long offset, CancellationToken cancellationToken)
    {
        using var timing = SonnetMqProcessingMetrics.Measure(subscription.Kind switch
        {
            SonnetMqEventKinds.TelemetryData => SonnetMqProcessingMetrics.TelemetryAck,
            SonnetMqEventKinds.AttributeData => SonnetMqProcessingMetrics.AttributeAck,
            _ => null
        });
        await _client.AckAsync(subscription.Topic, _options.ConsumerGroup, offset, cancellationToken);
    }

    private async Task PublishDeadLetterAsync(
        SonnetMqSubscription subscription,
        SndbMqMessage message,
        Exception exception,
        int attempts,
        CancellationToken cancellationToken)
    {
        string deadLetterTopic = subscription.Topic + _options.DeadLetterTopicSuffix;
        var deadLetter = new SonnetMqDeadLetterMessage(
            subscription.Topic,
            message.Offset,
            subscription.Kind,
            message.TimestampUtc,
            DateTimeOffset.UtcNow,
            attempts,
            message.Headers,
            exception.GetType().FullName ?? exception.GetType().Name,
            exception.Message,
            Truncate(exception.ToString(), Math.Max(256, _options.DeadLetterStackTraceMaxChars)),
            Convert.ToBase64String(message.Payload));

        byte[] payload = Encoding.UTF8.GetBytes(JsonObjectSerializer.Serialize(deadLetter));
        await _client.PublishAsync(deadLetterTopic, payload, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["content-type"] = "application/json",
            ["x-sonnetmq-dead-letter"] = "true"
        }, cancellationToken);
    }

    private void ClearDeliveryFailure(SndbMqMessage message)
        => _deliveryFailures.TryRemove(DeliveryFailureKey(message), out _);

    private static string DeliveryFailureKey(SndbMqMessage message)
        => message.Topic + ":" + message.Offset.ToString(System.Globalization.CultureInfo.InvariantCulture);

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
        return subscriber.Active(status.DeviceId, status.Activity, status.EventTimeUtc);
    }

    private static T ReadJson<T>(SndbMqMessage message)
    {
        string json = Encoding.UTF8.GetString(message.Payload);
        return JsonObjectSerializer.Deserialize<T>(json)
            ?? throw new InvalidDataException($"SonnetMQ payload cannot be deserialized as {typeof(T).Name}.");
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..maxLength];
    }

    private sealed record SonnetMqDeadLetterMessage(
        string SourceTopic,
        long SourceOffset,
        string Kind,
        DateTimeOffset SourceTimestampUtc,
        DateTimeOffset FailedAtUtc,
        int Attempts,
        IReadOnlyDictionary<string, string> Headers,
        string ErrorType,
        string ErrorMessage,
        string StackTrace,
        string PayloadBase64);
}
