using DotNetCore.CAP;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    public interface IEventBusHandler
    {
        public void StoreAttributeData(RawMsg msg);

        public void StoreTelemetryData(RawMsg msg);
    }

    /// <summary>
    /// iotsharp.services.datastream
    /// </summary>
    ///<remarks>Note: The injection of services needs before of `services.AddCap()`</remarks>
    ///
    public class EventBusHandler : IEventBusHandler, ICapSubscribe
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IStorage _storage;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;

        public EventBusHandler(ILogger<EventBusHandler> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, IStorage storage, FlowRuleProcessor flowRuleProcessor, IEasyCachingProviderFactory factory
            )
        {
            _appSettings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _storage = storage;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider("iotsharp");
        }
        Dictionary<Guid, DateTime> _check_device_status = new Dictionary<Guid, DateTime>();
        [CapSubscribe("iotsharp.services.datastream.attributedata")]
        public async void StoreAttributeData(RawMsg msg)
        {

            using (var _scope = _scopeFactor.CreateScope())
            {
                using (var _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var device = _dbContext.Device.FirstOrDefault(d => d.Id == msg.DeviceId);
                    if (device != null)
                    {
                        device.CheckOrUpdateDevStatus();
                        var mb = msg.MsgBody;
                        Dictionary<string, object> dc = new Dictionary<string, object>();
                        mb.ToList().ForEach(kp =>
                        {
                            if (kp.Value.GetType() == typeof(System.Text.Json.JsonElement))
                            {
                                var je = (System.Text.Json.JsonElement)kp.Value;
                                switch (je.ValueKind)
                                {
                                    case System.Text.Json.JsonValueKind.Undefined:
                                    case System.Text.Json.JsonValueKind.Object:
                                    case System.Text.Json.JsonValueKind.Array:
                                        dc.Add(kp.Key, je.GetRawText());
                                        break;

                                    case System.Text.Json.JsonValueKind.String:
                                        dc.Add(kp.Key, je.GetString());
                                        break;

                                    case System.Text.Json.JsonValueKind.Number:
                                        dc.Add(kp.Key, je.GetDouble());
                                        break;

                                    case System.Text.Json.JsonValueKind.True:
                                    case System.Text.Json.JsonValueKind.False:
                                        dc.Add(kp.Key, je.GetBoolean());
                                        break;

                                    case System.Text.Json.JsonValueKind.Null:
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                dc.Add(kp.Key, kp.Value);
                            }
                        });
                        var result2 = await _dbContext.SaveAsync<AttributeLatest>(dc, device.Id, msg.DataSide);
                        result2.exceptions?.ToList().ForEach(ex =>
                        {
                            _logger.LogError($"{ex.Key} {ex.Value} {Newtonsoft.Json.JsonConvert.SerializeObject(msg.MsgBody[ex.Key])}");
                        });
                        _logger.LogInformation($"更新{device.Name}({device.Id})属性数据结果{result2.ret}");
                    }
                }
            }
        }

        [CapSubscribe("iotsharp.services.datastream.telemetrydata")]
        public async void StoreTelemetryData(RawMsg msg)
        {

            await _storage.StoreTelemetryAsync(msg);
            if (!_check_device_status.ContainsKey(msg.DeviceId))
            {
                _check_device_status.Add(msg.DeviceId, DateTime.Now);
            }
            if (_check_device_status[msg.DeviceId].Subtract(DateTime.Now).TotalSeconds > 60)
            {
                _check_device_status[msg.DeviceId] = DateTime.Now;
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var device = _dbContext.Device.FirstOrDefault(d => d.Id == msg.DeviceId);
                    if (device != null)
                    {
                        device.CheckOrUpdateDevStatus();
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}