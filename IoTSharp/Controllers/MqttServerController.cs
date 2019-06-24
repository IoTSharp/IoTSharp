using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Handlers;
using IoTSharp.MQTT;
using IoTSharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.AspNetCoreEx;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Server.Status;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MqttServerController : ControllerBase
    {
        private readonly MQTTServerHandler _mqttService;

        public MqttServerController(MQTTServerHandler mqttService)
        {
            _mqttService = mqttService;
        }
        [HttpPost]
        public async Task Publish(string topic, string Payload, int qos = 0, bool retain = false)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            await _mqttService.Publish(new MqttPublishParameters<string>() { Payload = Payload, QualityOfServiceLevel = (MqttQualityOfServiceLevel)qos, Retain = retain, Topic = topic });
        }

        [HttpGet]
        public List<string> Imports()
        {
            return _mqttService.GetTopicImportUids();
        }

        [HttpPost("{uid}")]
        public void Import(string uid, [FromBody] MqttImportTopicParameters parameters)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            _mqttService.StartTopicImport(uid, parameters);
        }

        [HttpDelete("{uid}")]
      
        [ApiExplorerSettings(GroupName = "v1")]
        public void DeleteImport(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            _mqttService.StopTopicImport(uid);
        }

        [HttpGet]
        public Task<IList<IMqttClientStatus>>  Clients()
        {
            return _mqttService.GetClientsAsync();
        }

        [HttpGet]
        public Task<IList<IMqttSessionStatus>> Sessions()
        {
            return _mqttService.GetSessionsAsync();
        }

        [HttpGet]
  
        public Dictionary<string, MqttSubscriberModel>  Subscriptions()
        {
            return _mqttService.GetSubscribers().ToDictionary(s => s.Uid, s => new MqttSubscriberModel
            {
                TopicFilter = s.TopicFilter
            });
        }

        [HttpDelete("{uid}")]
       
        public void  Subscriber(string uid)
        {
            _mqttService.Unsubscribe(uid);
        }

        [HttpGet]
        public Task<IList<MqttApplicationMessage>>  RetainedMessages()
        {
            return _mqttService.GetRetainedMessagesAsync();
        }

        [HttpDelete("{topic}")]
        public async Task  RetainedMessage(string topic)
        {
         await   _mqttService.Publish(new MqttPublishParameters<byte[]>
            {
                Topic = topic,
                Payload = new byte[0],
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce,
                Retain = true
            });
        }

        [HttpDelete]
        public Task CleanRetainedMessages()
        {
            return _mqttService.DeleteRetainedMessagesAsync();
        }
    }
    public class MqttSubscriberModel
    {
        public string TopicFilter { get; set; }
    }
}
