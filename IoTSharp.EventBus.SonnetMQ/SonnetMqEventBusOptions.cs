namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqEventBusOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ConsumerGroup { get; set; } = "iotsharp";

    public int PullBatchSize { get; set; } = 256;

    public int IdleDelayMilliseconds { get; set; } = 100;

    public int ActiveCoalesceDefaultSeconds { get; set; } = 300;

    public int ActiveTimeoutCacheSeconds { get; set; } = 300;

    public int MaxDeliveryAttempts { get; set; } = 3;

    public string DeadLetterTopicSuffix { get; set; } = ".errors";

    public int DeadLetterStackTraceMaxChars { get; set; } = 8192;
}
