using DotNetCore.CAP;
using Dynamitey.DynamicObjects;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCore.AttributeRouting;
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
        private readonly ICapPublisher _queue;
        private readonly Device _dev;
        private string _devname;
        private Device device;

        public AlarmController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, ICapPublisher queue)
        {
            _logger = logger;
            _scopeFactor = scopeFactor;
            _queue = queue;
            _dev = Lazy.Create(async () => await GetSessionDataAsync<Device>(nameof(Device)));
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
                device = _dev.JudgeOrCreateNewDevice(devname, _scopeFactor, _logger);
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