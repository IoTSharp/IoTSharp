using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace IoTSharp.Gateways
{
    public class RawDataGateway 
    {
        private const string _map_to_telemety_ = "_map_to_telemetry_";
        private const string _map_to_attribute_ = "_map_to_attribute_";
        private const string _map_to_devname = "_map_to_devname";
        private const string _map_to_jsontext_in_json = "_map_to_jsontext_in_json";
        private const string _map_to_data_in_array = "_map_to_data_in_array";
        private const string _map_to_subdevname = "_map_to_subdevname";
        private const string _map_var_devname = "$devname";
        private const string _map_var_subdevname = "$subdevname";
        private const string _map_to_devname_format = "_map_to_devname_format";
        private const string _map_to_ = "_map_to_";
        private const string _map_var_ts_format = "_map_var_ts_format";
        private const string _map_var_ts_field = "_map_var_ts_field";
        private readonly AppSettings _setting;
        private readonly ILogger _logger;
        private readonly IPublisher _queue;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProvider _caching;
        private readonly ApplicationDbContext _context;

        public RawDataGateway(ILogger<RawDataGateway> logger, IServiceScopeFactory scopeFactor
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
 
        public async Task<ApiResult> ExecuteAsync(Device _dev, string format, string body)
        {
           await _queue.PublishActive(_dev.Id, ActivityStatus.Activity);
            var result=new ApiResult();
            string json = body;
            if (format == "xml")
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(body);
                json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
            }
            var atts_cach = await _caching.GetAsync($"_map_{_dev.Id}", async () =>
            {
                var guids = from al in _context.AttributeLatest where al.DeviceId == _dev.Id && al.KeyName.StartsWith(_map_to_) select al;
                return await guids.ToArrayAsync();
            }
         , TimeSpan.FromSeconds(_setting.RuleCachingExpiration));
            if (atts_cach.HasValue)
            {
                try
                {
                    var jroot = JToken.Parse(json);
                    JToken jt = null;
                    var atts = atts_cach.Value;
                    var pathx = atts.FirstOrDefault(al => al.KeyName == _map_to_jsontext_in_json)?.Value_String;
                    if (pathx != null)
                    {
                        _logger.LogWarning($"数据在{pathx}中以文本格式存放，在这里选中并转换为json格式");
                        jt = JToken.Parse(jroot.SelectToken(pathx).ToObject<string>());
                    }
                    else
                    {
                        jt = jroot;
                    }
                    var data_in_array = atts.FirstOrDefault(al => al.KeyName == _map_to_data_in_array)?.Value_String;
                    var ts_format = atts.FirstOrDefault(g => g.KeyName == _map_var_ts_format)?.Value_String ?? string.Empty;
                    var ts_field = atts.FirstOrDefault(g => g.KeyName == _map_var_ts_field)?.Value_String ?? string.Empty;
                    if (!string.IsNullOrEmpty(data_in_array))
                    {
                        var subdevname = atts.FirstOrDefault(al => al.KeyName == _map_to_subdevname)?.Value_String;
                        var jary = jt.SelectToken(data_in_array) as JArray;
                        if (jary == null)
                        {
                            _logger.LogWarning($"指定了数据在{data_in_array}中，但它为空或者不是数组。");
                            result = new ApiResult(ApiCode.CantFindObject, $"Can't found a arryay   by {data_in_array} ");
                        }
                        else
                        {
                            int errortimes = 0;
                            jary.Children().ForEach(jo =>
                            {
                                string _devname = buid_dev_name(atts, jt, jo);
                                if (!string.IsNullOrEmpty(_devname))
                                {
                                    push_one_device_data_with_json(jo, jt, _dev, _devname, atts, ts_field, ts_format);
                                }
                                else
                                {
                                    errortimes++;
                                }

                            });
                            if (errortimes>0)
                            {
                                result = new ApiResult(ApiCode.InValidData, $"can't found device name(times:{errortimes})");
                            }
                            else
                            {
                                result = new ApiResult(ApiCode.Success, "OK");
                            }
                        }
                    }
                    else
                    {
                        string _devname = buid_dev_name(atts, jt, null);
                        if (!string.IsNullOrEmpty(_devname))
                        {
                            push_one_device_data_with_json(jt, jroot, _dev, _devname, atts, ts_field, ts_format);
                            result = new ApiResult(ApiCode.Success, "OK");
                        }
                        else
                        {
                            result = new ApiResult(ApiCode.InValidData, "can't found device name");
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    result= new ApiResult(ApiCode.Exception, ex.Message);
                }
            }
            else
            {
                _logger.LogInformation($"{_dev}的数据不符合规范， 也无相关规则链处理。");
                result = new ApiResult(ApiCode.InValidData, $"{_dev}的数据不符合规范， 也无相关规则链处理。");
            }
            return result;
        }

        private string buid_dev_name(AttributeLatest[] atts, JToken jt, JToken jc)
        {
            string _result = string.Empty;
            var devnamekey = atts.FirstOrDefault(g => g.KeyName == _map_to_devname);
            var subdevnamekey = atts.FirstOrDefault(g => g.KeyName == _map_to_subdevname);
            if (devnamekey != null)
            {
                var devnameformatkey = atts.FirstOrDefault(g => g.KeyName == _map_to_devname_format)?.Value_String ?? _map_var_devname;

                var devname = string.Empty;
                if (devnamekey.Value_String.StartsWith('@'))
                {
                    devname = jt.SelectToken(devnamekey.Value_String[1..])?.ToObject<string>();
                }
                else if (jc!=null)
                {
                    devname = jc.SelectToken(devnamekey.Value_String)?.ToObject<string>();
                }
                else
                {
                    devname = jt.SelectToken(devnamekey.Value_String)?.ToObject<string>();
                }
                var subdevname = (subdevnamekey != null && (jc != null)) ? (jc.SelectToken(subdevnamekey.Value_String) as JValue)?.ToObject<string>() : string.Empty;
                if (!string.IsNullOrEmpty(devnameformatkey))
                {
                    _result = devnameformatkey;
                    if (!string.IsNullOrEmpty(devname)) _result = _result.Replace(_map_var_devname, devname);
                    if (!string.IsNullOrEmpty(subdevname)) _result = _result.Replace(_map_var_subdevname, subdevname);
                }
                else
                {
                    _result = $"{devname}{subdevname}";
                }
            }
            return _result;
        }

        private async void push_one_device_data_with_json(JToken jt, JToken jroot, Device _dev, string _devname, AttributeLatest[] atts, string ts_field, string ts_format)
        {
            var device = _dev.JudgeOrCreateNewDevice(_devname, _scopeFactor, _logger);
            var pairs_att = new Dictionary<string, object>();
            var pairs_tel = new Dictionary<string, object>();
            DateTime ts = DateTime.UtcNow;

            atts?.ToList().ForEach(g =>
            {
                JValue jv = null;
                if (g.Value_String.StartsWith("@"))//如果是@开头， 则从父节点取
                {
                    jv = jroot.SelectToken(g.Value_String.Substring(1)) as JValue;
                }
                else
                {
                    jv = jt.SelectToken(g.Value_String) as JValue;
                }
                var value = (jv)?.JValueToObject();
                if (value != null && g.KeyName?.Length > 0)
                {
                    if (!string.IsNullOrEmpty(ts_field))
                    {
                        if (g.KeyName == $"{_map_to_attribute_}{ts_field}" || g.KeyName == $"{_map_to_telemety_}{ts_field}")
                        {
                            if (!string.IsNullOrEmpty(ts_format))
                            {
                                if (!DateTime.TryParseExact(value as string, ts_format, null, System.Globalization.DateTimeStyles.None, out ts))
                                {
                                    ts = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                if (!DateTime.TryParse(value as string, out ts))
                                {
                                    ts = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                    if (g.KeyName.StartsWith(_map_to_attribute_) && g.KeyName.Length > _map_to_attribute_.Length)
                    {
                        pairs_att.Add(g.KeyName.Substring(_map_to_attribute_.Length), value);
                    }
                    else if (g.KeyName.StartsWith(_map_to_telemety_) && g.KeyName.Length > _map_to_telemety_.Length)
                    {
                        pairs_tel.Add(g.KeyName.Substring(_map_to_telemety_.Length), value);
                    }
                }
            });

            if (pairs_tel.Any())
            {
                await _queue.PublishTelemetryData(new PlayloadData() { ts = ts, DeviceId = device.Id, MsgBody = pairs_tel, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
            }
            if (pairs_att.Any())
            {
                await _queue.PublishAttributeData(new PlayloadData() { ts = ts, DeviceId = device.Id, MsgBody = pairs_att, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
            }
            await _queue.PublishActive(device.Id, ActivityStatus.Activity);



        }
    }
}
