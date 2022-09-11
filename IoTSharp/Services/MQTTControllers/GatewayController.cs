using DotNetCore.CAP;
using Dynamitey.DynamicObjects;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Gateways;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCore.AttributeRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("[controller]")]
    [MqttRoute("v1/[controller]")]
    public class GatewayController : MqttBaseController
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

        public GatewayController(ILogger<GatewayController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
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

        [MqttRoute("telemetry")]
        public Task telemetry()
        {
            var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<GatewayPlayload>>>(Message.ConvertPayloadToString());
            _logger.LogInformation($"{ClientId}的数据{Message.Topic}是网关数据， 解析到{lst?.Count}个设备");
            lst?.Keys.ToList().ForEach(dev =>
            {
                var plst = lst[dev];
                _logger.LogInformation($"{ClientId}的网关数据正在处理设备{dev}， 设备ID为{device?.Id}");
                plst.ForEach(p =>
                {
                    _queue.PublishTelemetryData(new PlayloadData() { DeviceId = device.Id, DeviceStatus = p.DeviceStatus, ts = new DateTime(p.Ticks), MsgBody = p.Values, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                });
                _logger.LogInformation($"{ClientId}的网关数据处理完成，设备{dev}ID为{device?.Id}共计{plst.Count}条");
            });
            return Ok();
        }

        [MqttRoute("attributes")]
        public Task Attributes()
        {
            var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<GatewayPlayload>>>(Message.ConvertPayloadToString());
            _logger.LogInformation($"{ClientId}的数据{Message.Topic}是网关数据， 解析到{lst?.Count}个设备");
            lst?.Keys.ToList().ForEach(dev =>
            {
                var plst = lst[dev];
                _logger.LogInformation($"{ClientId}的网关数据正在处理设备{dev}， 设备ID为{device?.Id}");
                plst.ForEach(p =>
                {
                    _queue.PublishAttributeData(new PlayloadData() { DeviceId = device.Id, DeviceStatus = p.DeviceStatus, ts = new DateTime(p.Ticks), MsgBody = p.Values, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                });
                _logger.LogInformation($"{ClientId}的网关数据处理完成，设备{dev}ID为{device?.Id}共计{plst.Count}条");
            });
            return Ok();
        }

        [MqttRoute("connect")]
        public Task on_connect()
        {
            var ds = Newtonsoft.Json.JsonConvert.DeserializeObject<GatewayDeviceStatus>(Message.ConvertPayloadToString());
            if (ds != null)
            {
                var device = _dev.JudgeOrCreateNewDevice(ds.Device, _scopeFactor, _logger);
                if (device != null)
                {
                    _queue.PublishDeviceStatus(device.Id, DeviceStatus.Good);
                }
                else
                {
                    _logger.LogWarning("未能创建或者找到网关的设备。");
                }
            }
            else
            {
                _logger.LogWarning("无法获取网关的子设备。");
            }
            return Ok();
        }

        [MqttRoute("disconnect")]
        public Task Disconnect()
        {
            var ds = Newtonsoft.Json.JsonConvert.DeserializeObject<GatewayDeviceStatus>(Message.ConvertPayloadToString());
            if (ds != null)
            {
                var device = _dev.JudgeOrCreateNewDevice(ds.Device, _scopeFactor, _logger);
                if (device != null)
                {
                    _queue.PublishDeviceStatus(device.Id, DeviceStatus.Bad);
                }
                else
                {
                    _logger.LogWarning("未能创建或者找到网关的设备。");
                }
            }
            else
            {
                _logger.LogWarning("无法获取网关的子设备。");
            }
            return Ok();
        }

        [MqttRoute("xml")]
        public async Task UploadXmlData()
        {
            try
            {
                using var sc = _scopeFactor.CreateScope();
                var hg = sc.ServiceProvider.GetService<RawDataGateway>();
                var result = await hg.ExecuteAsync(_dev, "xml", Message.ConvertPayloadToString());
                _logger.LogInformation($"调用XML网关处理语句返回:{result.Code}-{result.Msg}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"调用XML网关失败:{ex.Message}");
            }
        }

        [MqttRoute("json")]
        public async Task UploadJsonData()
        {
            try
            {
                using var sc = _scopeFactor.CreateScope();
                var hg = sc.ServiceProvider.GetService<RawDataGateway>();
                var result = await hg.ExecuteAsync(_dev, "json", Message.ConvertPayloadToString());
                _logger.LogInformation($"调用Json网关处理语句返回:{result.Code}-{result.Msg}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"调用Json网关失败:{ex.Message}");
            }
        }
    }
}