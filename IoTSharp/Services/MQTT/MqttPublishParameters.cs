using MQTTnet.Protocol;

namespace IoTSharp.MQTT
{
    public class MqttPublishParameters<T>
    {
        public string Topic { get; set; }

        public T Payload { get; set; } =default(T);

        public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; } = MqttQualityOfServiceLevel.AtMostOnce;

        public bool Retain { get; set; }
    }
}
