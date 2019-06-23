namespace IoTSharp.MQTT
{
    public class MqttServiceOptions
    {
        public const string Filename = "MqttServiceConfiguration.json";

        public bool EnableLogging { get; set; } = false;

        public bool PersistRetainedMessages { get; set; } = true;

        public int ServerPort { get; set; } = 1883;
    }
}
