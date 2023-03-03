using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esprima.Ast;

namespace IoTSharp.Services.MQTTControllers
{
    [MqttController]
    [MqttRoute("devices/{devname}/[controller]")]
    public class AttributesController : MqttBaseController
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
        private Device device;

        public AttributesController(ILogger<TelemetryController> logger, IServiceScopeFactory scopeFactor, MQTTService mqttService,
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
        public Task attributes_xml(string keyname)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(Message.ConvertPayloadToString());
                keyValues.Add(keyname, xml);
                _queue.PublishAttributeData(device, keyValues);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }

        [MqttRoute("binary/{keyname}")]
        public Task attributes_binary(string keyname)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                keyValues.Add(keyname, Message.Payload);
                _queue.PublishAttributeData(device, keyValues);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }

        // Supports template routing with typed constraints
        [MqttRoute()]
        public Task attributes()
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                if (Message.Payload?.Length > 0)
                {
                    keyValues = Message.ConvertPayloadToDictionary();
                    _queue.PublishAttributeData(device, keyValues);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
            return Ok();
        }

        [MqttRoute("request/{keyname}/{requestid}/xml")]
        public async Task RequestXML(string keyname, string requestid)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var qf = from at in _dbContext.AttributeLatest where at.Type == DataType.XML && at.KeyName == keyname select at;
                    await qf.LoadAsync();
                    await Server.PublishAsync(ClientId, $"devices/me/attributes/response/{requestid}", qf.FirstOrDefault()?.Value_XML);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
        }

        [MqttRoute("request/{keyname}/{requestid}/binary")]
        public async Task RequestBinary(string keyname, string requestid)
        {

            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var qf = from at in _dbContext.AttributeLatest where at.Type == DataType.Binary && at.KeyName == keyname select at;
                    await qf.LoadAsync();
                    await Server.PublishAsync(ClientId, $"devices/me/attributes/response/{requestid}", qf.FirstOrDefault()?.Value_Binary);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
        }

        [MqttRoute("request/{requestid}")]
        public async Task RequestAttributes(string requestid)
        {
            var reqlist = Message.ConvertPayloadToList<string>();
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    Dictionary<string, object> reps = new Dictionary<string, object>();
                    var reqid = requestid;
                    List<AttributeLatest> datas = new List<AttributeLatest>();
                    var qf = from at in _dbContext.AttributeLatest where at.DeviceId == device.Id && reqlist.Contains(at.KeyName) select at.AttributeToKeyValue();
                    var _dts = await qf.ToListAsync();
                    foreach (var kv in _dts)
                    {
                        reps.Add(kv.Key,kv.Value);
                    }
                    await Server.PublishAsync(ClientId, $"devices/me/attributes/response/{requestid}", reps);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{ex.Message}");
            }
        }

       
    }
}