using DotNetCore.CAP;
using Dynamitey.DynamicObjects;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
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
    /// <summary>
    /// 兼容thingsboard协议
    /// </summary>
    [MqttController]
    [MqttRoute("v1/gateway")]
    public class V1GatewayController : GatewayController
    {
        public V1GatewayController(ILogger<GatewayController> logger, IServiceScopeFactory scopeFactor,
         IOptions<AppSettings> options, ICapPublisher queue
         ) : base(logger, scopeFactor, options, queue)
        {
        }
    }

    [MqttController]
    [MqttRoute("[controller]")]
    public class GatewayController : MqttBaseController
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly ICapPublisher _queue;
        private readonly Device _dev;

        public GatewayController(ILogger<GatewayController> logger, IServiceScopeFactory scopeFactor,
            IOptions<AppSettings> options, ICapPublisher queue
            )
        {
            _logger = logger;
            _scopeFactor = scopeFactor;
            _queue = queue;
            _dev = Lazy.Create(async () => await GetSessionDataAsync<Device>(nameof(Device)));
        }

        [MqttRoute("telemetry")]
        public Task telemetry()
        {
            var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<GatewayPlayload>>>(Message.ConvertPayloadToString());
            _logger.LogInformation($"{ClientId}的数据{Message.Topic}是网关数据， 解析到{lst?.Count}个设备");
            lst?.Keys.ToList().ForEach(dev =>
            {
                var plst = lst[dev];
                var device = _dev.JudgeOrCreateNewDevice(dev, _scopeFactor, _logger);
                _logger.LogInformation($"{ClientId}的网关数据正在处理设备{dev}， 设备ID为{_dev?.Id}");
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
                var device = _dev.JudgeOrCreateNewDevice(dev, _scopeFactor, _logger);
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