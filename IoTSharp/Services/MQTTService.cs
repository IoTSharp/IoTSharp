using IoTSharp.EventBus;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Extensions;
using IoTSharp.FlowRuleEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IoTSharp.Services
{
    public class MQTTService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IEasyCachingProviderFactory _factory;
        private readonly MqttServer _serverEx;
        private readonly IPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly MqttClientSetting _mcsetting;
        private readonly AppSettings _settings;

        public MQTTService(ILogger<MQTTService> logger, IServiceScopeFactory scopeFactor, MqttServer serverEx
           , IOptions<AppSettings> options, IPublisher queue, IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor
            )
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _mcsetting = options.Value.MqttClient;
            _settings = options.Value;
            _logger = logger;
            _scopeFactor = scopeFactor;
            _factory = factory;
            _serverEx = serverEx;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider(_hc_Caching);
        }

        private static long clients = 0;

        internal Task Server_ClientConnectedAsync(ClientConnectedEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] {e.Endpoint} {e.UserName}  connected");
            clients++;
            return Task.CompletedTask;
        }

        private static DateTime uptime = DateTime.MinValue;

        internal Task Server_Started(EventArgs e)
        {
            _logger.LogInformation($"MqttServer is  started");
            uptime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        internal Task Server_Stopped(EventArgs e)
        {
            _logger.LogInformation($"Server is stopped");
            return Task.CompletedTask;
        }

        private async Task<Device> FoundDevice(string clientid)
        {
            Device device = null;
            var clients = await _serverEx.GetClientsAsync();
            var client = clients.FirstOrDefault(c => c.Id == clientid);
            if (client != null)
            {
                device = client.Session.Items[nameof(Device)] as Device;
                if (device == null)
                {
                    if (clientid != _mcsetting.MqttBroker)
                    {
                        _logger.LogWarning($"未能找到客户端{clientid}回话附加的设备信息，现在断开此链接。 ");
                        await client.DisconnectAsync();
                    }
                }
            }
            else
            {
                _logger.LogWarning($"未能找到客户端{clientid}上下文信息");
            }
            return device;
        }

        internal async Task Server_ClientDisconnected(ClientDisconnectedEventArgs args)
        {
            try
            {
                var dev = args.SessionItems[nameof(Device)] as Device;
                if (dev != null)
                {
                    await _queue.PublishConnect(dev.Id, ConnectStatus.Disconnected);
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

        private long Subscribed;

        internal Task Server_ClientSubscribedTopic(ClientSubscribedTopicEventArgs e)
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

        internal Task Server_ClientConnectionValidator(ValidatingConnectionEventArgs e)
        {
            try
            {
                using (var scope = _scopeFactor.CreateScope())
                using (var _dbContextcv = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var obj = e;

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
                    if (isLoopback && !string.IsNullOrEmpty(e.ClientId) && e.ClientId == _mcsetting.MqttBroker && !string.IsNullOrEmpty(e.UserName))
                    {
                        e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                    }
                    else
                    {
                        string _thumbprint = string.Empty ;
                        if (_settings.MqttBroker.EnableTls)
                        {
                            _thumbprint = e.ClientCertificate?.Thumbprint;
                        }
                            _logger.LogInformation($"ClientId={obj.ClientId},Endpoint={obj.Endpoint},Username={obj.UserName}，Password={obj.Password}");
                        var mcr = _dbContextcv.DeviceIdentities.Include(d => d.Device).FirstOrDefault(mc =>
                                              (mc.IdentityType == IdentityType.AccessToken && mc.IdentityId == obj.UserName) ||
                                             ( mc.IdentityType == IdentityType.X509Certificate &&  mc.IdentityId == _thumbprint ) ||
                                             ( mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.UserName && mc.IdentityValue == obj.Password)
                                             );
                        if (mcr != null)
                        {
                            try
                            {
                                var device = mcr.Device;
                                e.SessionItems.Add(nameof(Device), device);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                                _queue.PublishConnect(device.Id, ConnectStatus.Connected);
                                _logger.LogInformation($"Device {device.Name}({device.Id}) is online !username is {obj.UserName} and  is endpoint{obj.Endpoint}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "ConnectionRefusedServerUnavailable {0}", ex.Message);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ServerUnavailable;
                            }
                        }
                        else if (_dbContextcv.AuthorizedKeys.Any(ak => ak.AuthToken == obj.Password && ak.Deleted==false))
                        {
                            var ak = _dbContextcv.AuthorizedKeys.Include(ak => ak.Customer).Include(ak => ak.Tenant).Include(ak => ak.Devices).FirstOrDefault(ak => ak.AuthToken == obj.Password && ak.Deleted==false);
                            if (ak != null && !ak.Devices.Any(dev => dev.Name == obj.UserName))
                            {
                                var devvalue = new Device() { Name = obj.UserName, DeviceType = DeviceType.Device, Timeout = 300 };
                                devvalue.Tenant = ak.Tenant;
                                devvalue.Customer = ak.Customer;
                                _dbContextcv.Device.Add(devvalue);
                                ak.Devices.Add(devvalue);
                                _dbContextcv.AfterCreateDevice(devvalue, obj.UserName, obj.Password);
                                _dbContextcv.SaveChanges();
                                _queue.PublishConnect(devvalue.Id,  ConnectStatus.Connected);
                            }
                            var mcp = _dbContextcv.DeviceIdentities.Include(d => d.Device).FirstOrDefault(mc => mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.UserName && mc.IdentityValue == obj.Password);
                            if (mcp != null)
                            {
                                e.SessionItems.Add(nameof(Device), mcp.Device);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                                _queue.PublishConnect(mcp.Device.Id, ConnectStatus.Disconnected);
                                _logger.LogInformation($"Device {mcp.Device.Name}({mcp.Device.Id}) is online !username is {obj.UserName} and  is endpoint{obj.Endpoint}");
                            }
                            else
                            {
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                                _logger.LogInformation($"Bad username or  password/AuthToken {obj.UserName},connection {obj.Endpoint} refused");
                            }
                        }
                        else if (_dbContextcv.Produces.Any(ak =>  obj.UserName.StartsWith( ak.Name+"_")  &&   ak.ProduceToken == obj.Password && ak.Deleted == false))
                        {
                            var ak = _dbContextcv.Produces.Include(ak => ak.Customer).Include(ak => ak.Tenant).Include(ak => ak.Devices).FirstOrDefault(ak =>  ak.ProduceToken == obj.Password && ak.Deleted == false);
                            if (ak!=null &&   ak.Devices.Any(d => d.Name == obj.UserName && d.Deleted == false))
                            {
                                var devvalue = new Device() { Name = obj.UserName, DeviceType = ak.DefaultDeviceType, Timeout = ak.DefaultTimeout };
                                devvalue.Tenant = ak.Tenant;
                                devvalue.Customer = ak.Customer;
                                _dbContextcv.Device.Add(devvalue);
                                ak.Devices.Add(devvalue);
                                _dbContextcv.AfterCreateDevice(devvalue,ak.Id);
                                _dbContextcv.SaveChanges();
                                _queue.PublishConnect(devvalue.Id, ConnectStatus.Connected);
                            }
                            var mcp = ak.Devices.FirstOrDefault(d => d.Name == obj.UserName && d.Deleted == false);
                            if (mcp != null)
                            {
                                e.SessionItems.Add(nameof(Device), mcp);
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                                _queue.PublishConnect(mcp.Id, ConnectStatus.Disconnected);
                                _logger.LogInformation($"Produce {ak.Name}'s   device {mcp.Name}({mcp.Id}) is online ! Client name  is {obj.UserName} and  is endpoint{obj.Endpoint}");
                            }
                            else
                            {
                                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                                _logger.LogInformation($"Bad device name  or  ProduceToken {obj.UserName},connection {obj.Endpoint} refused");
                            }
                        }
                        else
                        {
                            e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                            _logger.LogInformation($"Bad username or password {obj.UserName},connection {obj.Endpoint} refused");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ImplementationSpecificError;
                e.ReasonString = ex.Message;
                _logger.LogError(ex, "ImplementationSpecificError {0}", ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}