namespace IoTSharp.EventBus.SonnetMQ;

internal static class SonnetMqEventBusTopics
{
    public const string AttributeData = "iotsharp.services.datastream.attributedata";
    public const string TelemetryData = "iotsharp.services.datastream.telemetrydata";
    public const string Alarm = "iotsharp.services.datastream.alarm";
    public const string CreateDevice = "iotsharp.services.platform.createdevice";
    public const string DeleteDevice = "iotsharp.services.platform.deletedevice";
    public const string Connect = "iotsharp.services.platform.connect";
    public const string Active = "iotsharp.services.platform.active";

    public static IReadOnlyList<SonnetMqSubscription> Subscriptions { get; } =
    [
        new(AttributeData, SonnetMqEventKinds.AttributeData),
        new(TelemetryData, SonnetMqEventKinds.TelemetryData),
        new(Alarm, SonnetMqEventKinds.Alarm),
        new(CreateDevice, SonnetMqEventKinds.CreateDevice),
        new(DeleteDevice, SonnetMqEventKinds.DeleteDevice),
        new(Connect, SonnetMqEventKinds.Connect),
        new(Active, SonnetMqEventKinds.Active)
    ];
}

internal static class SonnetMqEventKinds
{
    public const string AttributeData = "attribute-data";
    public const string TelemetryData = "telemetry-data";
    public const string Alarm = "alarm";
    public const string CreateDevice = "create-device";
    public const string DeleteDevice = "delete-device";
    public const string Connect = "connect";
    public const string Active = "active";
}

internal sealed record SonnetMqSubscription(string Topic, string Kind);
