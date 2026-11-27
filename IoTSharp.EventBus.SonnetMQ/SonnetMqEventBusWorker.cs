using System.Text;
using System.Text.Json;
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
        var progress = new SubscriptionProgress();
        bool orderedData = subscription.Kind is SonnetMqEventKinds.TelemetryData or SonnetMqEventKinds.AttributeData;
        while (!stoppingToken.IsCancellationRequested)
        {
            bool confirming = progress.PendingAck.HasValue;
            try
            {
                // ACK 响应丢失时保留已完成的位点，只重发确认，不重新写入或执行规则。
                if (progress.PendingAck is long offset)
                {
                    long next = await AckAsync(subscription, offset, stoppingToken);
                    progress.PendingAck = null;
                    confirming = false;
                    if (orderedData && next != offset + 1)
                    {
                        _logger.LogCritical(
                            "SonnetMQ subscription halted for reconciliation. Topic={Topic}, expectedOffset={ExpectedOffset}, confirmedOffset={ConfirmedOffset}.",
                            subscription.Topic, offset + 1, next);
                        return;
                    }
                    progress.NextOffset = next;
                }

                if (progress.Halted)
                    return;

                if (orderedData && progress.NextOffset is null)
                {
                    var stats = await _client.GetStatsAsync(subscription.Topic, stoppingToken);
                    progress.NextOffset = stats.ConsumerOffsets.GetValueOrDefault(
                        _options.ConsumerGroup, stats.NextOffset - stats.MessageCount);
                }

                bool processed = await PullAndDispatchAsync(subscription, progress, stoppingToken);
                recoveryDelaySeconds = 1;
                if (!processed)
                    await Task.Delay(Math.Max(1, _options.IdleDelayMilliseconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (!confirming && SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.Partial)
            {
                _logger.LogCritical(
                    "SonnetMQ subscription halted for reconciliation. Topic={Topic}, firstUnconfirmedOffset={Offset}, error={Error}.",
                    subscription.Topic, progress.DispatchOffset, ex.InnerException?.GetType().Name ?? SonnetDbWriteRecovery.Partial);
                progress.Halted = true;
            }
            catch (Exception ex) when (!confirming && SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.Rejected)
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

    private async Task<bool> PullAndDispatchAsync(
        SonnetMqSubscription subscription, SubscriptionProgress progress, CancellationToken cancellationToken)
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
        if (progress.NextOffset is long expected && subscription.Kind is SonnetMqEventKinds.TelemetryData or SonnetMqEventKinds.AttributeData)
        {
            progress.DispatchOffset = expected;
            for (int index = 0; index < messages.Count; index++)
            {
                if (messages[index].Offset != expected + index)
                {
                    progress.Halted = true;
                    _logger.LogCritical(
                        "SonnetMQ subscription halted for reconciliation. Topic={Topic}, expectedOffset={ExpectedOffset}, returnedOffset={ReturnedOffset}.",
                        subscription.Topic, expected + index, messages[index].Offset);
                    return true;
                }
            }
        }

        if (messages.Count == 0)
            return false;

        progress.DispatchOffset = messages[0].Offset;
        if (subscription.Kind == SonnetMqEventKinds.TelemetryData)
            await DispatchTelemetryBatchAsync(subscription, messages, progress, cancellationToken);
        else
            await DispatchMessagesIndividuallyAsync(subscription, messages, progress, cancellationToken);

        return true;
    }

    private async Task DispatchMessagesIndividuallyAsync(
        SonnetMqSubscription subscription,
        IReadOnlyList<SndbMqMessage> messages,
        SubscriptionProgress progress,
        CancellationToken cancellationToken)
    {
        bool isData = subscription.Kind == SonnetMqEventKinds.AttributeData;
        using var scope = _scopeFactory.CreateScope();
        var subscriber = scope.ServiceProvider.GetRequiredService<ISubscriber>();
        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.DispatchOffset = message.Offset;
            PlayloadData? attribute = null;
            if (isData)
            {
                try
                {
                    attribute = ReadData(message);
                }
                catch (Exception ex) when (ex is JsonException or InvalidDataException)
                {
                    await HandleDispatchFailureAsync(subscription, message, progress, ex, cancellationToken);
                    return;
                }
            }

            try
            {
                using var timing = SonnetMqProcessingMetrics.Measure(isData ? SonnetMqProcessingMetrics.AttributeDispatch : null);
                using var recovery = SonnetDbWriteRecovery.Enable();
                if (attribute is not null)
                    await subscriber.StoreAttributeData(attribute);
                else
                    await DispatchAsync(subscriber, subscription.Kind, message);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                && SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.None)
            {
                if (isData)
                    throw SonnetDbWriteRecovery.AfterEarlierWrites(ex);

                await HandleDispatchFailureAsync(subscription, message, progress, ex, cancellationToken);
                return;
            }

            progress.Complete(message.Offset);
        }
    }

    private async Task DispatchTelemetryBatchAsync(
        SonnetMqSubscription subscription,
        IReadOnlyList<SndbMqMessage> messages,
        SubscriptionProgress progress,
        CancellationToken cancellationToken)
    {
        using var dispatch = SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryBatchDispatch);
        var payloads = new PlayloadData[messages.Count];
        int decoded = 0;
        Exception? decodeError = null;
        using (SonnetMqProcessingMetrics.Measure(SonnetMqProcessingMetrics.TelemetryBatchDecode))
        {
            try
            {
                for (; decoded < payloads.Length; decoded++)
                    payloads[decoded] = ReadData(messages[decoded]);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException)
            {
                decodeError = ex;
            }
        }

        if (decoded != 0)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var recovery = SonnetDbWriteRecovery.Enable();
                using var scope = _scopeFactory.CreateScope();
                var subscriber = scope.ServiceProvider.GetRequiredService<ISubscriber>();
                await subscriber.StoreTelemetryDataBatch(new ArraySegment<PlayloadData>(payloads, 0, decoded));
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                && SonnetDbWriteRecovery.Classify(ex) == SonnetDbWriteRecovery.Outcome.None)
            {
                throw SonnetDbWriteRecovery.AfterEarlierWrites(ex);
            }
            progress.Complete(messages[decoded - 1].Offset);
        }

        if (decodeError is not null)
        {
            progress.DispatchOffset = messages[decoded].Offset;
            await HandleDispatchFailureAsync(subscription, messages[decoded], progress, decodeError, cancellationToken);
        }
    }

    private async Task HandleDispatchFailureAsync(
        SonnetMqSubscription subscription,
        SndbMqMessage message,
        SubscriptionProgress progress,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int maxAttempts = Math.Max(1, _options.MaxDeliveryAttempts);
        if (progress.FailedOffset != message.Offset)
        {
            progress.FailedOffset = message.Offset;
            progress.FailedAttempts = 0;
        }
        if (progress.FailedAttempts < maxAttempts)
            progress.FailedAttempts++;
        int attempts = progress.FailedAttempts;

        if (attempts < maxAttempts)
        {
            _logger.LogWarning(
                exception,
                "SonnetMQ event dispatch failed. It will be retried. topic={Topic}, offset={Offset}, attempt={Attempt}, maxAttempts={MaxAttempts}",
                message.Topic,
                message.Offset,
                attempts,
                maxAttempts);
            return;
        }

        await PublishDeadLetterAsync(subscription, message, exception, attempts, cancellationToken);
        progress.Complete(message.Offset);
        _logger.LogError(
            exception,
            "SonnetMQ event dispatch failed permanently and was moved to dead letter. topic={Topic}, offset={Offset}, attempts={Attempts}",
            message.Topic,
            message.Offset,
            attempts);
    }

    /// <summary>确认已完成的位点并记录遥测或属性的确认耗时。</summary>
    private async Task<long> AckAsync(SonnetMqSubscription subscription, long offset, CancellationToken cancellationToken)
    {
        using var timing = SonnetMqProcessingMetrics.Measure(subscription.Kind switch
        {
            SonnetMqEventKinds.TelemetryData => SonnetMqProcessingMetrics.TelemetryAck,
            SonnetMqEventKinds.AttributeData => SonnetMqProcessingMetrics.AttributeAck,
            _ => null
        });
        long nextOffset = await _client.AckAsync(subscription.Topic, _options.ConsumerGroup, offset, cancellationToken);
        if (nextOffset <= offset)
            throw new InvalidOperationException($"SonnetMQ ACK did not confirm offset {offset}. NextOffset={nextOffset}.");
        return nextOffset;
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

    private sealed class SubscriptionProgress
    {
        public long? PendingAck { get; set; }
        public long? NextOffset { get; set; }
        public long DispatchOffset { get; set; }
        public long? FailedOffset { get; set; }
        public int FailedAttempts { get; set; }
        public bool Halted { get; set; }

        public void Complete(long offset)
        {
            PendingAck = offset;
            if (FailedOffset <= offset)
                FailedOffset = null;
        }
    }

    private static Task DispatchAsync(ISubscriber subscriber, string kind, SndbMqMessage message)
        => kind switch
        {
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
        return JsonSerializer.Deserialize<T>(message.Payload, JsonOptions.Default)
            ?? throw new InvalidDataException($"SonnetMQ payload cannot be deserialized as {typeof(T).Name}.");
    }

    private static PlayloadData ReadData(SndbMqMessage message)
    {
        var data = ReadJson<PlayloadData>(message);
        return data.MsgBody is not null ? data : throw new InvalidDataException("SonnetMQ data payload has no message body.");
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
