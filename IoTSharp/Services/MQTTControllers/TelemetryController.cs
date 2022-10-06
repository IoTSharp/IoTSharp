using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTSharp.Contracts;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class TelemetryController : MqttBaseController
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IPublisher _queue;
        private string _devname;
        private Device device;

        public TelemetryController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, IPublisher queue)
        {

            _logger = logger;
            _scopeFactor = scopeFactor;
            _queue = queue;

        }

        public string devname
        {
            get
            {
                return _devname;
            }
            set
            {
                _devname = value;
                var _dev = GetSessionItem<Device>();
                device = _dev.JudgeOrCreateNewDevice(devname, _scopeFactor, _logger);
                _queue.PublishActive(_dev.Id, ActivityStatus.Activity);
                if (_dev.DeviceType == DeviceType.Gateway)
                {
                    _queue.PublishActive(device.Id, ActivityStatus.Activity);
                }
            }
        }

        [MqttRoute("xml/{keyname}")]
        public Task telemetry_xml(string keyname)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(Message.ConvertPayloadToString());
                keyValues.Add(keyname, xml);
                _queue.PublishTelemetryData(device, keyValues);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }

        [MqttRoute("binary/{keyname}")]
        public Task telemetry_binary(string keyname)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                keyValues.Add(keyname, Message.Payload);
                _queue.PublishTelemetryData(device, keyValues);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }

        [MqttRoute()]
        public Task telemetry()
        {
            try
            {
                if (Message.Payload?.Length > 0)
                {
                    var keyValues = Message.ConvertPayloadToDictionary();
                    _queue.PublishTelemetryData(device, keyValues);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }
    }
}