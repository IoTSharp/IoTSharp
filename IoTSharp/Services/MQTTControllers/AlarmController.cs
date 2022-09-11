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
        private readonly IEasyCachingProviderFactory _factory;
        private readonly ICapPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly Device _dev;
        private readonly MQTTService _service;
        private readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;
        private string _devname;
        private Device device;

        public AlarmController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
            IOptions<AppSettings> options, ICapPublisher queue, IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor
            )
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _mcsetting = options.Value.MqttClient;
            _settings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _factory = factory;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider(_hc_Caching);
            _dev = Lazy.Create(async () => await GetSessionDataAsync<Device>(nameof(Device)));
            _service = mqttService;
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
            _queue.PublishDeviceAlarm(dto);
            return Ok();
        }
    }
}