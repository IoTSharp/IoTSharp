using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using MQTTnet.AspNetCore.AttributeRouting;
using DotNetCore.CAP;
using EasyCaching.Core;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MQTTnet.Server;
using IoTSharp.Data;
using Dynamitey.DynamicObjects;
using Amazon.SimpleNotificationService.Model;
using System.Collections.Generic;
using MQTTnet;
using IoTSharp.Extensions;
using NATS.Client;
using static IronPython.Modules._ast;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class StatusController : MqttBaseController
    {
        readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProviderFactory _factory;
        private readonly ICapPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly Device _dev;
        private readonly MQTTService _service;
        readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;
        private string _devname;
        private Device device;

        public StatusController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
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
        [MqttRoute("{status}")]
        public Task UpdateStatus(DeviceStatus status)
        {
            _logger.LogInformation($"重置状态{device.Id} {device.Name}");
            if (device.DeviceType == DeviceType.Device && device.Owner != null && device.Owner?.Id != null && device.Owner?.Id != Guid.Empty)//虚拟设备上线
            {
                _queue.PublishDeviceStatus(device.Id, status);
                _queue.PublishDeviceStatus(device.Owner.Id, status != DeviceStatus.Good ? DeviceStatus.PartGood : status);
                _logger.LogInformation($"重置网关状态{device.Owner.Id} {device.Owner.Name}");
            }
            else
            {
                _queue.PublishDeviceStatus(device.Id, status);
            }
            return Ok();
        }
     

  
    }
}
