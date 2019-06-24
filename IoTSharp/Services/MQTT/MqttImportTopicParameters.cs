using MQTTnet.Protocol;

namespace IoTSharp.MQTT
{
    public class MqttImportTopicParameters
    {
        public string Server { get; set; }

        public int? Port { get; set; }

        public string ClientId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Topic { get; set; }

        public bool UseTls { get; set; }

        public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; } = MqttQualityOfServiceLevel.AtMostOnce;
    }
}
