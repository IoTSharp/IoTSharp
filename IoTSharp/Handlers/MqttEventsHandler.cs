using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.X509Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.AspNetCoreEx;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    public class MqttEventsHandler
    {
        readonly ILogger<MqttEventsHandler> _logger;
        readonly ApplicationDbContext _dbContext;
        readonly IMqttServerEx _serverEx;
        public MqttEventsHandler(ILogger<MqttEventsHandler> logger, ApplicationDbContext dbContext, IMqttServerEx serverEx)
        {
            _logger = logger;
            _dbContext = dbContext;
            _serverEx = serverEx;
        }

        static long clients = 0;
        internal void Server_ClientConnected(object sender, MqttServerClientConnectedEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] connected");
            clients++;
            Task.Run(() => _serverEx.PublishAsync("$SYS/broker/clients/total", clients.ToString()));
        }
        static DateTime uptime = DateTime.MinValue;
        internal void Server_Started(object sender, EventArgs e)
        {
            _logger.LogInformation($"MqttServer is  started");
            uptime = DateTime.Now;
        }

        internal void Server_Stopped(object sender, EventArgs e)
        {
            _logger.LogInformation($"Server is stopped");
        }
        Dictionary<string, int> lstTopics = new Dictionary<string, int>();
        long received = 0;

        internal void Server_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"Server received {e.ClientId}'s message: Topic=[{e.ApplicationMessage.Topic }],Retain=[{e.ApplicationMessage.Retain}],QualityOfServiceLevel=[{e.ApplicationMessage.QualityOfServiceLevel}]");
            if (!lstTopics.ContainsKey(e.ApplicationMessage.Topic))
            {
                lstTopics.Add(e.ApplicationMessage.Topic, 1);
                Task.Run(() => _serverEx.PublishAsync("$SYS/broker/subscriptions/count", lstTopics.Count.ToString()));
            }
            else
            {
                lstTopics[e.ApplicationMessage.Topic]++;
            }
            if (e.ApplicationMessage.Payload != null)
            {
                received += e.ApplicationMessage.Payload.Length;
            }
            string topic = e.ApplicationMessage.Topic;
            var tpary = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (tpary.Length >= 3 && tpary[0] == "devices" && Devices.ContainsKey(e.ClientId))
            {
                Device device = JudgeOrCreateNewDevice(tpary, Devices[e.ClientId]);
                var  keyValues = e.ApplicationMessage.ConvertPayloadToDictionary();
                if (device != null)
                {
                    if (tpary[2] == "telemetry")
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                var result = await _dbContext.SaveAsync<TelemetryLatest, TelemetryData>(keyValues, device, DataSide.ClientSide);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Can't upload telemetry to device {device.Name}({device.Id}).the payload is {e.ApplicationMessage.ConvertPayloadToString()}");
                            }
                        });
                    }
                    else if (tpary[2] == "attributes")
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                              
                                var result = await _dbContext.SaveAsync<AttributeLatest, AttributeData>(keyValues, device, DataSide.ClientSide);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Can't upload attributes to device {device.Name}({device.Id}).the payload is \"{e.ApplicationMessage.ConvertPayloadToString()}\"");
                            }
                        });

                    }
                }
            }




        }

        internal void Server_ClientDisconnected(IMqttServerEx server, MqttServerClientDisconnectedEventArgs args)
        {
            try
            {
                Devices.Remove(args.ClientId);
            }
            catch (Exception)
            {

             
            }
        }

        private Device JudgeOrCreateNewDevice(string[] tpary, Device device)
        {
            Device devicedatato = device;
            if (tpary[1] != "me" && device.DeviceType == DeviceType.Gateway)
            {
                var ch = from g in _dbContext.Gateway.Include(c => c.Children) where g.Id == device.Id select g;
                var gw = ch.FirstOrDefault();
                var subdev = from cd in  gw.Children where cd.Name == tpary[1] select cd;
                if (!subdev.Any())
                {
                    devicedatato = new Device() { Id = Guid.NewGuid(), Name = tpary[1], DeviceType = DeviceType.Device, Tenant = device.Tenant, Customer = device.Customer };
                    gw.Children.Add(devicedatato);
                    _dbContext.SaveChangesAsync();
                }
                else
                {
                    devicedatato = subdev.FirstOrDefault();
                }
            }
            return devicedatato;
        }

        long Subscribed;
        internal void Server_ClientSubscribedTopic(object sender, MqttServerClientSubscribedTopicEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] subscribed [{e.TopicFilter}]");
            if (e.TopicFilter.Topic.StartsWith("$SYS/"))
            {
                if (e.TopicFilter.Topic.StartsWith("$SYS/broker/version"))
                {
                    var mename = typeof(MqttEventsHandler).Assembly.GetName();
                    var mqttnet = typeof(MqttServerClientSubscribedTopicEventArgs).Assembly.GetName();
                    Task.Run(() => _serverEx.PublishAsync("$SYS/broker/version", $"{mename.Name}V{mename.Version.ToString()},{mqttnet.Name}.{mqttnet.Version.ToString()}"));
                }
                else if (e.TopicFilter.Topic.StartsWith("$SYS/broker/uptime"))
                {
                    Task.Run(() => _serverEx.PublishAsync("$SYS/broker/uptime", uptime.ToString()));
                }
            }
            if (e.TopicFilter.Topic.ToLower().StartsWith("/devices/telemetry"))///devices/attributes
            {


            }
            else
            {
                Subscribed++;
                Task.Run(() => _serverEx.PublishAsync("$SYS/broker/subscriptions/count", Subscribed.ToString()));
            }


        }

        internal void Server_ClientUnsubscribedTopic(object sender, MqttServerClientUnsubscribedTopicEventArgs e)
        {
            _logger.LogInformation($"Client [{e.ClientId}] unsubscribed[{e.TopicFilter}]");
            if (!e.TopicFilter.StartsWith("$SYS/"))
            {
                Subscribed--;
                Task.Run(() => _serverEx.PublishAsync("$SYS/broker/subscriptions/count", Subscribed.ToString()));
            }
        }
        internal static Dictionary<string, Device> Devices = new Dictionary<string, Device>();
        public static string MD5Sum(string text) => BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
        internal void Server_ClientConnectionValidator(object sender, MqttServerClientConnectionValidatorEventArgs e)
        {
            MqttConnectionValidatorContext obj = e.Context;
            Uri uri = new Uri("mqtt://" + obj.Endpoint);
            if (string.IsNullOrEmpty(obj.Username) && uri.IsLoopback)
            {
                obj.ReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted;
                _logger.LogInformation($"Loopback {obj.Endpoint}， ConnectionAccepted");
            }
            else
            {
                _logger.LogInformation($"ClientId={obj.ClientId},Endpoint={obj.Endpoint},Username={obj.Username}，Password={obj.Password},WillMessage={obj.WillMessage?.ConvertPayloadToString()}");
                var mcr = _dbContext.DeviceIdentities.Include(d=>d.Device).FirstOrDefault(mc => (mc.IdentityType == IdentityType.AccessToken && mc.IdentityId == obj.Username)
                                                                            || (mc.IdentityType == IdentityType.DevicePassword && mc.IdentityId == obj.Username && mc.IdentityValue == obj.Password));
                if (mcr != null)
                {
                    try
                    {
                        var device = mcr.Device;
                        if (!Devices.ContainsKey(e.Context.ClientId))
                        {
                            Devices.Add(e.Context.ClientId, device);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ConnectionRefusedServerUnavailable {0}", ex.Message);
                        obj.ReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedServerUnavailable;
                    }

                }
                else
                {
                    obj.ReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    _logger.LogInformation($"Bad username or password {obj.Username},connection {obj.Endpoint} refused");
                }
            }

        }
    }
}
