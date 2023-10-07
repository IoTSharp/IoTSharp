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
using System.Threading.Tasks;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class RpcController : MqttBaseController
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

        public RpcController(ILogger<RpcController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
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

        [MqttRoute("request/{method}")]
        public async Task request(string method)
        {
            _logger.LogInformation($"收到客户端{ClientId}rpc请求方法{method}。");
            var p_dev = _dev.DeviceType == DeviceType.Gateway ? device : _dev;
            var rules = await _caching.GetAsync($"ruleid_{p_dev.Id}_rpc_{method}", async () =>
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var guids = await _dbContext.GerDeviceRpcRulesList(p_dev.Id, EventType.RPC, method);
                    return guids;
                }
            }
            , TimeSpan.FromSeconds(_settings.RuleCachingExpiration));
            if (rules.HasValue && rules.Value!=Guid.Empty)
            {
                var obj = new { Message.Topic, Payload = Convert.ToBase64String(Message.Payload), ClientId, RPCMethod = method };
                _logger.LogInformation($"客户端{ClientId}rpc请求方法{method}通过规则链{rules.Value}进行处理。");
                await _flowRuleProcessor.RunFlowRules(rules.Value, obj, p_dev.Id, FlowRuleRunType.Normal, null);
            }
            else
            {
                _logger.LogInformation($"客户端{ClientId}rpc请求方法{method}尚未委托规则链。");
            }
        }
    }
}