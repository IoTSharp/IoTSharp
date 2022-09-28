using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class StatusController : MqttBaseController
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IPublisher _queue;
        private string _devname;
        private Device device;

        public StatusController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, IPublisher queue)
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