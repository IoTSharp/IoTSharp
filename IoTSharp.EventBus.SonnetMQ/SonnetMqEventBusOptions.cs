namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqEventBusOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ConsumerGroup { get; set; } = "iotsharp";

    public int PullBatchSize { get; set; } = 256;

    public int IdleDelayMilliseconds { get; set; } = 100;
}
