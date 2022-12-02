using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using IoTSharp.Extensions;

namespace IoTSharp.Gateways
{

    public class OPCUATag
    {
        /// <summary>
        /// 模拟器示例.函数._System._DemandPoll
        /// </summary>
        [JsonPropertyName("id")]
        public string TagName { get; set; }

        public List<string> TagPath => TagName?.Split('.').ToList();

        public bool IsSystem => (TagPath?.Count==4 && TagPath[2]== "_System");

        public string DeviceName => TagPath[1];
        public string TelemetryName => IsSystem? TagPath[3]:TagPath[2];

        public bool CanUse => (TagPath?.Count()).GetValueOrDefault() >= 2;
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("v")]
        public JsonElement TagValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("q")]
        public bool TagQuality { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("t")]
        public long  TagTimestamp { get; set; }
    }

    public class KepStandardTemplate
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        public DateTime DateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
        [JsonPropertyName("values")]
        public List<OPCUATag> Tags { get; set; }
    }


    //    id: |TAGNAME|
    //v: |TAGVALUE|
    //q: |TAGQUALITY|
    //t: |TAGTIMESTAMP|
    public class KepServerEx
    {
        private readonly AppSettings _setting;
        private readonly ILogger _logger;
        private readonly IPublisher _queue;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProvider _caching;
        private readonly ApplicationDbContext _context;

        public KepServerEx(ILogger<KepServerEx> logger, IServiceScopeFactory scopeFactor
           , IOptions<AppSettings> options, IPublisher queue, IEasyCachingProviderFactory factory
            , ApplicationDbContext context
            )
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _setting = options.Value;
            _logger = logger;
            _queue = queue;
            _scopeFactor = scopeFactor;
            _caching = factory.GetCachingProvider(_hc_Caching);
            _context = context;

        }

        public async Task<ApiResult> ExecuteAsync(Device _dev, byte[] body)
        {
            await _queue.PublishActive(_dev.Id, ActivityStatus.Activity);
            var kp = System.Text.Json.JsonSerializer.Deserialize<KepStandardTemplate>(body);
            var devs = from d in kp.Tags where d.CanUse && !d.IsSystem select d.DeviceName;
            var lst = devs.Distinct();
            foreach (var dev in lst)
            {
                var device = _dev.JudgeOrCreateNewDevice(dev, _scopeFactor, _logger);
                await _queue.PublishActive(device.Id, ActivityStatus.Activity);
                _logger.LogInformation($"{_dev.Name}的网关数据正在处理设备{dev}， 设备ID为{_dev?.Id}");
                var plst = from d in kp.Tags where d.CanUse &&  d.DeviceName == dev select new KeyValuePair<string, object>(d.TelemetryName, d.TagValue.ToObject());

                if (plst.Any())
                {
                    await _queue.PublishTelemetryData(new PlayloadData()
                    {
                        DeviceId = device.Id,
                        ts = kp.DateTime,
                        MsgBody = new Dictionary<string, object>(plst.DistinctBy(k=>k.Key)),
                        DataSide = DataSide.ClientSide,
                        DataCatalog = DataCatalog.TelemetryData
                    });
                }
                _logger.LogInformation($"{_dev.Name}的网关数据处理完成，设备{dev}ID为{device?.Id}共计{plst.Count()}条");
            }
            var _sys = from d in kp.Tags where d.CanUse &&  d.IsSystem  select new KeyValuePair<string, object>(d.TelemetryName, d.TagValue.ToObject());
            if (_sys.Any())
            {
                await _queue.PublishTelemetryData(new PlayloadData()
                {
                    DeviceId = _dev.Id,
                    ts = kp.DateTime,
                    MsgBody = new Dictionary<string, object>(_sys.DistinctBy(k=>k.Key)),
                    DataSide = DataSide.ClientSide,
                    DataCatalog = DataCatalog.TelemetryData
                });
            }
            _logger.LogInformation($"{_dev.Name}的网关数据处理完成，设备的系统属性共计{_sys.Count()}条");
            return new ApiResult() { Code = (int)ApiCode.Success, Msg = "OK" };
        }
    }

}