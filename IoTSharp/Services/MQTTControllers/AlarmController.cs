using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class AlarmController : MqttBaseController
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IPublisher _queue;

        private string _devname;
        private Device device;

        public AlarmController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, IPublisher queue)
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
                _queue.PublishActive(_dev.Id,  ActivityStatus.Activity);
            }
        }

        [MqttRoute()]
        public Task UpdateStatus()
        {
            var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateAlarmDto>(Message.ConvertPayloadToString());
            dto.OriginatorName = device.Name;
            _queue.PublishDeviceAlarm(dto);
            return Ok();
        }
    }
}