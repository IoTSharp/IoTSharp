namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqEventBusOptions
{
    public string Path { get; set; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IoTSharp",
        "sonnetmq");

    public string ConsumerGroup { get; set; } = "iotsharp";

    public int PullBatchSize { get; set; } = 256;

    public int IdleDelayMilliseconds { get; set; } = 100;

    public bool FlushOnPublish { get; set; }

    public bool SyncOnPublish { get; set; }

    public int OffsetIndexStride { get; set; } = 1024;
}
