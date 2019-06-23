using System;
using MQTTnet;
using MQTTnet.Server;

namespace IoTSharp.MQTT
{
    public class MqttSubscriber
    {
        private readonly Action<MqttApplicationMessageReceivedEventArgs> _callback;

        public MqttSubscriber(string uid, string topicFilter, Action<MqttApplicationMessageReceivedEventArgs> callback)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            TopicFilter = topicFilter ?? throw new ArgumentNullException(nameof(topicFilter));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public string Uid { get; }

        public string TopicFilter { get; }

        public bool IsFilterMatch(string topic)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));

            return MqttTopicFilterComparer.IsMatch(topic, TopicFilter);
        }

        public void Notify(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            _callback(eventArgs);
        }
    }
}
