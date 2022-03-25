using DotNetCore.CAP;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    public class MQTTServerHandler
    {
        readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProviderFactory _factory;
        readonly MqttServer _serverEx;
        private readonly ICapPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;

        public MQTTServerHandler(ILogger<MQTTServerHandler> logger, IServiceScopeFactory scopeFactor, MqttServer serverEx
           , IOptions<AppSettings> options, ICapPublisher queue, IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor
            )
        {
            _mcsetting = options.Value.MqttClient;
            _settings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _factory = factory;
            _serverEx = serverEx;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider("iotsharp");
        }

        static long clients = 0;
        internal  Task   Server_ClientConnectedAsync(ClientConnectedEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] {e.Endpoint} {e.UserName}  connected");
            clients++;
            return Task.CompletedTask;
        }
        static DateTime uptime = DateTime.MinValue;
        internal Task Server_Started( EventArgs e)
        {
            _logger.LogInformation($"MqttServer is  started");
            uptime = DateTime.Now;
            return Task.CompletedTask;  
        }

        internal Task Server_Stopped(EventArgs e)
        {
            _logger.LogInformation($"Server is stopped");
            return Task.CompletedTask;
        }
        internal async Task Server_ApplicationMessageReceived(ApplicationMessageNotConsumedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SenderClientId))
            {
                _logger.LogInformation($"ClientId为空,无法进一步获取设备信息 Topic=[{e.ApplicationMessage.Topic }]");
            }
            else
            {
                try
                {
                    _logger.LogInformation($"Server received {e.SenderClientId}'s message: Topic=[{e.ApplicationMessage.Topic }],Retain=[{e.ApplicationMessage.Retain}],QualityOfServiceLevel=[{e.ApplicationMessage.QualityOfServiceLevel}]");
                    string topic = e.ApplicationMessage.Topic;
                    var tpary = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    var _dev = await FoundDevice(e.SenderClientId);
               
                    if (tpary.Length >= 3 && tpary[0] == "devices" && _dev != null)
                    {
                        var device = _dev.JudgeOrCreateNewDevice( tpary[1], _scopeFactor, _logger);
                        if (device != null)
                        {
                            bool statushavevalue = false;
                            Dictionary<string, object> keyValues = new Dictionary<string, object>();
                            if (tpary.Length >= 4)
                            {
                                string keyname = tpary.Length >= 5 ? tpary[4] : tpary[3];
                                if (tpary[3].ToLower() == "xml")
                                {
                                    try
                                    {
                                        var xml = new System.Xml.XmlDocument();
                                        xml.LoadXml(e.ApplicationMessage.ConvertPayloadToString());
                                        keyValues.Add(keyname, xml);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, $"xml data error {topic},{ex.Message}");
                                    }
                                }
                                else if (tpary[3].ToLower() == "binary")
                                {
                                    keyValues.Add(keyname, e.ApplicationMessage.Payload);
                                }
                            }
                            else
                            {
                                try
                                {
                                    keyValues = e.ApplicationMessage.ConvertPayloadToDictionary();
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"转换为字典格式失败 {topic},{ex.Message}");
                                }
                            }
                            if (tpary[2] == "telemetry")
                            {
                                _queue.PublishTelemetryData(new RawMsg() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                            }
                            else if (tpary[2] == "attributes")
                            {
                                if (tpary.Length > 3 && tpary[3] == "request")
                                {
                                    await RequestAttributes(tpary,e.SenderClientId, e.ApplicationMessage.ConvertPayloadToDictionary(), device);
                                }
                                else
                                {
                                    _queue.PublishAttributeData(new RawMsg() { DeviceId =  device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
                                }
                            }
                            else if (tpary[2] == "status" )
                            {
                                var ds = Enum.TryParse(tpary[3], true, out DeviceStatus status) ? status : (tpary[3] == "online" ? DeviceStatus.Good : DeviceStatus.Bad);
                                ResetDeviceStatus(device, ds);
                                statushavevalue = true;
                            }
                            else if (tpary[2] == "rpc")
                            {
                                if (tpary[3]== "request")
                                {
                                    await ExecFlowRules(e, _dev.DeviceType == DeviceType.Gateway ? device : _dev, tpary[4], MountType.RPC);//完善后改成 RPC 
                                }
                            }
                            else
                            {
                                await ExecFlowRules(e, _dev.DeviceType == DeviceType.Gateway ? device : _dev, MountType.RAW);//如果是网关
                            }
                            if (!statushavevalue)
                            {
                                ResetDeviceStatus(device);
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"{e.SenderClientId}的数据{e.ApplicationMessage.Topic}未能匹配到设备");
                        }
                    }
                    else if (tpary.Length >= 3 && tpary[0] == "gateway" && _dev != null  )
                    {
                        var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<Playload>>>(e.ApplicationMessage.ConvertPayloadToString());
                        _logger.LogInformation($"{e.SenderClientId}的数据{e.ApplicationMessage.Topic}是网关数据， 解析到{lst?.Count}个设备");
                        bool istelemetry =tpary[2] == "telemetry";
                        lst?.Keys.ToList().ForEach(dev =>
                        {

                            var plst = lst[dev];
                            var device = _dev.JudgeOrCreateNewDevice(dev, _scopeFactor, _logger);
                            _logger.LogInformation($"{e.SenderClientId}的网关数据正在处理设备{dev}， 设备ID为{device?.Id}");
                            plst.ForEach(p =>
                            {
                                if (istelemetry)
                                {
                                    _queue.PublishTelemetryData(new RawMsg() { DeviceId = device.Id, DeviceStatus = p.DeviceStatus, ts = new DateTime( p.Ticks), MsgBody = p.Values, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                                }
                                else
                                {
                                    _queue.PublishAttributeData(new RawMsg() { DeviceId = device.Id, DeviceStatus = p.DeviceStatus, ts = new DateTime(p.Ticks), MsgBody = p.Values, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
                                }
                            });
                            _logger.LogInformation($"{e.SenderClientId}的网关数据处理完成，设备{dev}ID为{device?.Id}共计{plst.Count}条");
                        });
                    }    
                    else
                    {
                        //tpary.Length >= 3 && tpary[0] == "devices" && _dev != null
                        _logger.LogWarning($"不支持{e.SenderClientId}的{e.ApplicationMessage.Topic}格式,Length:{tpary.Length },{tpary[0] },{ _dev != null}");
                    }
                }
                catch (Exception ex)
                {
                
                    _logger.LogWarning(ex, $"ApplicationMessageReceived {ex.Message} {ex.InnerException?.Message}");
                }

            }
        }

        private void ResetDeviceStatus(Device device, DeviceStatus status = DeviceStatus.Good)
        {
            _logger.LogInformation($"重置状态{device.Id} {device.Name}");
            if (device.DeviceType == DeviceType.Device && device.Owner != null && device.Owner?.Id != null && device.Owner?.Id !=Guid.Empty)//虚拟设备上线
            {
                _queue.PublishDeviceStatus(device.Id, status);
                _queue.PublishDeviceStatus(device.Owner.Id, status!= DeviceStatus.Good ? DeviceStatus.PartGood: status);
                _logger.LogInformation($"重置网关状态{device.Owner.Id} {device.Owner.Name}");
            }
            else
            {
                _queue.PublishDeviceStatus(device.Id, status);
            }
        }

        private async Task ExecFlowRules(ApplicationMessageNotConsumedEventArgs e, Device _dev, string method, MountType mount)
        {
            var rules = await _caching.GetAsync($"ruleid_{_dev.Id}_rpc_{method}", async () =>
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var guids = await _dbContext.GerDeviceRpcRulesList(_dev.Id, mount,method);
                    return guids;
                }
            }
            , TimeSpan.FromSeconds(_settings.RuleCachingExpiration));
            if (rules.HasValue)
            {
                var obj = new { e.ApplicationMessage.Topic, Payload = Convert.ToBase64String(e.ApplicationMessage.Payload), e.SenderClientId };
            
                    _logger.LogInformation($"{e.SenderClientId}的rpc调用{e.ApplicationMessage.Topic} 方法 {method}通过规则链{rules.Value}进行处理。");
                    await _flowRuleProcessor.RunFlowRules(rules.Value, obj, _dev.Id, EventType.Normal, null);
            }
            else
            {
                _logger.LogInformation($"{e.SenderClientId}的数据{e.ApplicationMessage.Topic}不符合规范， 也无相关规则链处理。");
            }
        }
        private async Task ExecFlowRules(ApplicationMessageNotConsumedEventArgs e, Device _dev, MountType mount)
        {
            var rules = await _caching.GetAsync($"ruleid_{_dev.Id}_raw", async () =>
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var guids = await _dbContext.GerDeviceRulesIdList(_dev.Id, mount);
                    return guids;
                }
            }
            , TimeSpan.FromSeconds(_settings.RuleCachingExpiration));
            if (rules.HasValue)
            {
                var obj = new { e.ApplicationMessage.Topic, Payload = Convert.ToBase64String(e.ApplicationMessage.Payload), e.SenderClientId };
                rules.Value.ToList().ForEach(async g =>
                {
                    _logger.LogInformation($"{e.SenderClientId}的数据{e.ApplicationMessage.Topic}通过规则链{g}进行处理。");
                    await _flowRuleProcessor.RunFlowRules(g, obj, _dev.Id, EventType.Normal, null);
                });
            }
            else
            {
                _logger.LogInformation($"{e.SenderClientId}的数据{e.ApplicationMessage.Topic}不符合规范， 也无相关规则链处理。");
            }
        }

        private async Task<Device> FoundDevice(string clientid)
        {
            Device device = null;
            var clients = await _serverEx.GetClientsAsync();
            var client = clients.FirstOrDefault(c => c.Id == clientid);
            if (client != null)
            {
                device =client.Session.Items[ nameof(Device)] as Device;
                if (device==null)
                {
                    if (clientid != _mcsetting.MqttBroker)
                    {
                        _logger.LogWarning($"未能找到客户端{clientid  }回话附加的设备信息，现在断开此链接。 ");
                        await client.DisconnectAsync();
                    }
                }
            }
            else
            {
                _logger.LogWarning($"未能找到客户端{clientid  }上下文信息");
            }
            return device;
        }

        private async Task RequestAttributes(string[] tpary, string senderClientId, Dictionary<string, object> keyValues, Device device)
        {
            using (var scope = _scopeFactor.CreateScope())
            using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                if (tpary.Length > 5 && tpary[4] == "xml")
                {
                    var qf = from at in _dbContext.AttributeLatest where at.Type == DataType.XML && at.KeyName == tpary[5] select at;
                    await qf.LoadAsync();
                    await _serverEx.PublishAsync(senderClientId,  $"devices/me/attributes/response/{tpary[5]}", qf.FirstOrDefault()?.Value_XML);
                }
                else if (tpary.Length > 5 && tpary[4] == "binary")
                {
                    var qf = from at in _dbContext.AttributeLatest where at.Type == DataType.Binary && at.KeyName == tpary[5] select at;
                    await qf.LoadAsync();
                    await _serverEx.PublishAsync(senderClientId, $"devices/me/attributes/response/{tpary[5]}", qf.FirstOrDefault()?.Value_Binary );
                }
                else
                {
                    Dictionary<string, object> reps = new Dictionary<string, object>();
                    var reqid = tpary.Length > 4 ? tpary[4] : Guid.NewGuid().ToString();
                    List<AttributeLatest> datas = new List<AttributeLatest>();
                    foreach (var kx in keyValues)
                    {
                        var keys = kx.Value?.ToString().Split(',');
                        if (keys != null && keys.Length > 0)
                        {
                            if (Enum.TryParse(kx.Key, true, out DataSide ds))
                            {
                                var qf = from at in _dbContext.AttributeLatest where at.DeviceId == device.Id && keys.Contains(at.KeyName) select at;
                                await qf.LoadAsync();
                                if (ds == DataSide.AnySide)
                                {
                                    datas.AddRange(await qf.ToArrayAsync());
                                }
                                else
                                {
                                    var qx = from at in qf where at.DataSide == ds select at;
                                    datas.AddRange(await qx.ToArrayAsync());
                                }
                            }
                        }
                    }


                    foreach (var item in datas)
                    {
                        switch (item.Type)
                        {
                            case DataType.Boolean:
                                reps.Add(item.KeyName, item.Value_Boolean);
                                break;
                            case DataType.String:
                                reps.Add(item.KeyName, item.Value_String);
                                break;
                            case DataType.Long:
                                reps.Add(item.KeyName, item.Value_Long);
                                break;
                            case DataType.Double:
                                reps.Add(item.KeyName, item.Value_Double);
                                break;
                            case DataType.Json:
                                reps.Add(item.KeyName, Newtonsoft.Json.Linq.JToken.Parse(item.Value_Json));
                                break;
                            case DataType.XML:
                                reps.Add(item.KeyName, item.Value_XML);
                                break;
                            case DataType.Binary:
                                reps.Add(item.KeyName, item.Value_Binary);
                                break;
                            case DataType.DateTime:
                                reps.Add(item.KeyName, item.Value_DateTime);
                                break;
                            default:
                                reps.Add(item.KeyName, item.Value_Json);
                                break;
                        }
                    }

                    await _serverEx.PublishAsync(senderClientId, $"devices/me/attributes/response/{tpary[4]}", reps);
                }
            }

        }

        internal async Task Server_ClientDisconnected( ClientDisconnectedEventArgs args)
        {
         
                try
                {
                    var dev = await FoundDevice(args.ClientId);
                    if (dev != null)
                    {
                        using (var scope = _scopeFactor.CreateScope())
                        using (var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                        {
                            var devtmp = _dbContext.Device.FirstOrDefault(d => d.Id == dev.Id);
                            devtmp.LastActive = DateTime.Now;
                            devtmp.Online = false;
                        await _dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Server_ClientDisconnected   ClientId:{args.ClientId} DisconnectType:{args.DisconnectType}  Device is {devtmp.Name }({devtmp.Id}) ");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Server_ClientDisconnected ClientId:{args.ClientId} DisconnectType:{args.DisconnectType}, 未能在缓存中找到");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Server_ClientDisconnected ClientId:{args.ClientId} DisconnectType:{args.DisconnectType},{ex.Message}");

                }
       
        }

      

        long Subscribed;
        internal    Task Server_ClientSubscribedTopic( ClientSubscribedTopicEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] subscribed [{e.TopicFilter}]");

            if (e.TopicFilter.Topic.StartsWith("$SYS/"))
            {
               
             
            }
            if (e.TopicFilter.Topic.ToLower().StartsWith("devices/telemetry"))
            {
              

            }
            else
            {
                Subscribed++;
            }
            return Task.CompletedTask;
        }

        internal Task Server_ClientUnsubscribedTopic(ClientUnsubscribedTopicEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] unsubscribed[{e.TopicFilter}]");
            if (!e.TopicFilter.StartsWith("$SYS/"))
            {
                Subscribed--;
            }
            return Task.CompletedTask; 
        }
         
         
       
        public static string MD5Sum(string text) => BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
        internal  Task Server_ClientConnectionValidator( ValidatingConnectionEventArgs e)
        {
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContextcv = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                     var  obj = e;

                    // jy 特殊处理 ::1
                    var isLoopback = false;
                    if (obj.Endpoint?.StartsWith("::1") == true)
                    {
                        isLoopback = true;
                    }
                    else
                    {
                        Uri uri = new Uri("mqtt://" + obj.Endpoint);
                        isLoopback = uri.IsLoopback;
                    }
                    if (isLoopback && !string.IsNullOrEmpty(e.ClientId) && e.ClientId == _mcsetting.MqttBroker && !string.IsNullOrEmpty(e.Username) )
                    {
                        e.ReasonCode   = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                    }
                    else
                    {
                        _logger.LogInformation($"ClientId={obj.ClientId},Endpoint={obj.Endpoint},Username={obj.Username}，Password={obj.Password}");
                        var mcr = _dbContextcv.DeviceIdentities.Include(d => d.Device).FirstOrDefault(mc =>
                                              (mc.IdentityType == IdentityType.AccessToken && mc.IdentityId == obj.Username) ||
                                              (mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.Username && mc.IdentityValue == obj.Password));
                        if (mcr != null)
                        {
                            try
                            {
                                var device = mcr.Device;
                              
                                e.SessionItems.Add(nameof(Device), device);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                                _logger.LogInformation($"Device {device.Name}({device.Id}) is online !username is {obj.Username} and  is endpoint{obj.Endpoint}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "ConnectionRefusedServerUnavailable {0}", ex.Message);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ServerUnavailable;
                            }
                        }
                        else if (_dbContextcv.AuthorizedKeys.Any(ak => ak.AuthToken == obj.Password))
                        {
                            var ak = _dbContextcv.AuthorizedKeys.Include(ak => ak.Customer).Include(ak => ak.Tenant).Include(ak => ak.Devices).FirstOrDefault(ak => ak.AuthToken == obj.Password);
                            if (ak != null && !ak.Devices.Any(dev => dev.Name == obj.Username))
                            {

                                var devvalue = new Device() { Name = obj.Username, DeviceType = DeviceType.Device, Timeout = 300, LastActive = DateTime.Now };
                                devvalue.Tenant = ak.Tenant;
                                devvalue.Customer = ak.Customer;
                                _dbContextcv.Device.Add(devvalue);
                                ak.Devices.Add(devvalue);
                                _dbContextcv.AfterCreateDevice(devvalue, obj.Username, obj.Password);
                                _dbContextcv.SaveChanges();
                            }
                            var mcp = _dbContextcv.DeviceIdentities.Include(d => d.Device).FirstOrDefault(mc => mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.Username && mc.IdentityValue == obj.Password);
                            if (mcp != null)
                            {
                                e.SessionItems.Add(nameof(Device), mcp.Device);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                                _logger.LogInformation($"Device {mcp.Device.Name}({mcp.Device.Id}) is online !username is {obj.Username} and  is endpoint{obj.Endpoint}");
                            }
                            else
                            {
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                                _logger.LogInformation($"Bad username or  password/AuthToken {obj.Username},connection {obj.Endpoint} refused");
                            }
                        }
                        else
                        {

                            e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                            _logger.LogInformation($"Bad username or password {obj.Username},connection {obj.Endpoint} refused");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ImplementationSpecificError;
                e.ReasonString = ex.Message;
                _logger.LogError(ex, "ConnectionRefusedServerUnavailable {0}", ex.Message);
            }
            return Task.CompletedTask;

        }


    }
}
