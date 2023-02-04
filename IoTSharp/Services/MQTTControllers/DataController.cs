using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class DataController : MqttBaseController
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProviderFactory _factory;
        private readonly IPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly MQTTService _service;
        private readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;
        private string _devname;
        private Device _dev;
        private Device device;

        public DataController(ILogger<DataController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
            IOptions<AppSettings> options, IPublisher queue, IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor
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
                _dev = GetSessionItem<Device>();
                device = _dev.JudgeOrCreateNewDevice(devname, _scopeFactor, _logger);
                _queue.PublishActive(_dev.Id, ActivityStatus.Activity);
                if (_dev.DeviceType == DeviceType.Gateway)
                {
                    _queue.PublishActive(device.Id, ActivityStatus.Activity);
                }
            }
        }
        [MqttRoute("binary")]
        public async Task BinaryDataProcessing()
        {
            var p_dev = _dev.DeviceType == DeviceType.Gateway ? device : _dev;
            var rules = await _caching.GetAsync($"ruleid_{p_dev.Id}_raw_binary", async () =>
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var guids = await _dbContext.GerDeviceRulesIdList(p_dev.Id, EventType.RAW);
                    return guids;
                }
            }
            , TimeSpan.FromSeconds(_settings.RuleCachingExpiration));
            if (rules.HasValue)
            {
                var obj =   Message.Payload ;
                rules.Value.ToList().ForEach(async g =>
                {
                    _logger.LogInformation($"{ClientId}的数据{Message.Topic}通过规则链{g}进行处理。");
                    await _flowRuleProcessor.RunFlowRules(g, obj, p_dev.Id, FlowRuleRunType.Normal, null);
                });
            }
            else
            {
                _logger.LogInformation($"{ClientId}的数据{Message.Topic}不符合规范， 也无相关规则链处理。");
            }
        }
        [MqttRoute("json")]
        public async Task JsonDataProcessing()
        {
            var p_dev = _dev.DeviceType == DeviceType.Gateway ? device : _dev;
            var rules = await _caching.GetAsync($"ruleid_{p_dev.Id}_raw_json", async () =>
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var guids = await _dbContext.GerDeviceRulesIdList(p_dev.Id, EventType.RAW);
                    return guids;
                }
            }
            , TimeSpan.FromSeconds(_settings.RuleCachingExpiration));
            if (rules.HasValue)
            {
                var obj = JsonConvert.DeserializeObject(Message.ConvertPayloadToString());
                rules.Value.ToList().ForEach(async g =>
                {
                    _logger.LogInformation($"{ClientId}的数据{Message.Topic}通过规则链{g}进行处理。");
                    await _flowRuleProcessor.RunFlowRules(g, obj, p_dev.Id, FlowRuleRunType.Normal, null);
                });
            }
            else
            {
                _logger.LogInformation($"{ClientId}的数据{Message.Topic}不符合规范， 也无相关规则链处理。");
            }
        }
    }
}