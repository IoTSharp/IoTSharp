using IoTSharp.Data;
using IoTSharp.Diagnostics;
using IoTSharp.Extensions;
using IoTSharp.MQTT;
using IoTSharp.Storage;
using IoTSharp.Sys;
using IoTSharp.X509Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.AspNetCoreEx;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    public class MqttEventsHandler
    {
        readonly ILogger<MqttEventsHandler> _logger;
        readonly ApplicationDbContext _dbContext;
        readonly IMqttServerEx _serverEx;
        private readonly BlockingCollection<MqttApplicationMessageReceivedEventArgs> _incomingMessages = new BlockingCollection<MqttApplicationMessageReceivedEventArgs>();
        private readonly Dictionary<string, MqttTopicImporter> _importers = new Dictionary<string, MqttTopicImporter>();
        private readonly Dictionary<string, MqttSubscriber> _subscribers = new Dictionary<string, MqttSubscriber>();
        private readonly OperationsPerSecondCounter _inboundCounter;
        private readonly OperationsPerSecondCounter _outboundCounter;


        public MqttEventsHandler(ILogger<MqttEventsHandler> logger, ApplicationDbContext dbContext, IMqttServerEx serverEx, DiagnosticsService diagnosticsService,
            SystemStatusService systemStatusService
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _serverEx = serverEx;

            _inboundCounter = diagnosticsService.CreateOperationsPerSecondCounter("mqtt.inbound_rate");
            _outboundCounter = diagnosticsService.CreateOperationsPerSecondCounter("mqtt.outbound_rate");

            systemStatusService.Set("mqtt.subscribers_count", () => _subscribers.Count);
            systemStatusService.Set("mqtt.incoming_messages_count", () => _incomingMessages.Count);
            systemStatusService.Set("mqtt.inbound_rate", () => _inboundCounter.Count);
            systemStatusService.Set("mqtt.outbound_rate", () => _outboundCounter.Count);
            systemStatusService.Set("mqtt.connected_clients_count", () => serverEx.GetClientStatusAsync().GetAwaiter().GetResult().Count);


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
            _inboundCounter.Increment();
            _incomingMessages.Add(e);
            if (string.IsNullOrEmpty(e.ClientId))
            {
                _logger.LogInformation($"Message: Topic=[{e.ApplicationMessage.Topic }]");
            }
            else
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
                    if (device != null)
                    {
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
                                _logger.LogWarning(ex, $"ConvertPayloadToDictionary   Error {topic},{ex.Message}");
                            }
                        }
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
                            if (tpary.Length > 3 && tpary[3] == "request")
                            {
                                Task.Run(async () =>
                                {
                                    await RequestAttributes(tpary, e.ApplicationMessage.ConvertPayloadToDictionary(), device);
                                });
                            }
                            else
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
            }
        }

        private async Task RequestAttributes(string[] tpary, Dictionary<string, object> keyValues,Device device)
        {
            if (tpary.Length>5 &&  tpary[4] == "xml" )
            {
                var qf = from at in _dbContext.AttributeData where at.Type == DataType.XML && at.KeyName == tpary[5] select at;
                await _serverEx.PublishAsync($"/devices/me/attributes/response/{tpary[5]}", qf.FirstOrDefault()?.Value_XML);
            }
            else if (tpary.Length > 5 &&  tpary[4] == "binary")
            {
                var qf = from at in _dbContext.AttributeData where at.Type == DataType.Binary && at.KeyName == tpary[5] select at;
                await _serverEx.PublishAsync(new MqttApplicationMessage() {  Topic = $"/devices/me/attributes/response/{tpary[5]}", Payload = qf.FirstOrDefault()?.Value_Binary });
            }
            else
            {
                Dictionary<string, object> reps = new Dictionary<string, object>();
                var reqid = tpary.Length > 4 ? tpary[4] : Guid.NewGuid().ToString();
                List<AttributeData> datas = new List<AttributeData>();
                foreach (var kx in keyValues)
                {
                    var keys = kx.Value?.ToString().Split(',');
                    if (keys != null && keys.Length > 0)
                    {
                        if (Enum.TryParse(kx.Key, true, out DataSide ds))
                        {
                            var qf = from at in _dbContext.AttributeData where at.Device== device &&  at.DataSide == ds && keys.Contains(at.KeyName) select at;
                            datas.AddRange(await qf.ToArrayAsync());
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
                await _serverEx.PublishAsync($"/devices/me/attributes/response/{reqid}", Newtonsoft.Json.JsonConvert.SerializeObject(reps));
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
                var ch = from g in _dbContext.Gateway.Include(g=>g.Tenant).Include(g=>g.Customer).Include(c => c.Children) where g.Id == device.Id select g;
                var gw = ch.FirstOrDefault();
                var subdev = from cd in  gw.Children where cd.Name == tpary[1] select cd;
                if (!subdev.Any())
                {
                    devicedatato = new Device() { Id = Guid.NewGuid(), Name = tpary[1], DeviceType = DeviceType.Device, Tenant = gw.Tenant, Customer = gw.Customer, Owner=gw };
                    gw.Children.Add(devicedatato);
                    _dbContext.AfterCreateDevice(devicedatato);
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
            if (e.TopicFilter.Topic.ToLower().StartsWith("/devices/telemetry"))
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

        public List<string> GetTopicImportUids()
        {
            lock (_importers)
            {
                return _importers.Select(i => i.Key).ToList();
            }
        }

        public string StartTopicImport(string uid, MqttImportTopicParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (string.IsNullOrEmpty(uid))
            {
                uid = Guid.NewGuid().ToString("D");
            }

            var importer = new MqttTopicImporter(parameters, this , _logger);
            importer.Start();

            lock (_importers)
            {
                if (_importers.TryGetValue(uid, out var existingImporter))
                {
                    existingImporter.Stop();
                }

                _importers[uid] = importer;
            }

            _logger.Log(LogLevel.Information, "Started importer '{0}' for topic '{1}' from server '{2}'.", uid, parameters.Topic, parameters.Server);
            return uid;
        }

        public void StopTopicImport(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_importers)
            {
                if (_importers.TryGetValue(uid, out var importer))
                {
                    importer.Stop();
                    _logger.Log(LogLevel.Information, "Stopped importer '{0}'.");
                }

                _importers.Remove(uid);
            }
        }

        public List<MqttSubscriber> GetSubscribers()
        {
            lock (_subscribers)
            {
                return _subscribers.Values.ToList();
            }
        }

        public Task<IList<MqttApplicationMessage>> GetRetainedMessagesAsync()
        {
            return _serverEx.GetRetainedApplicationMessagesAsync();
        }

        public Task DeleteRetainedMessagesAsync()
        {
            return _serverEx.ClearRetainedApplicationMessagesAsync();
        }

        public void Publish(MqttPublishParameters parameters)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(parameters.Topic)
                .WithPayload(parameters.Payload)
                .WithQualityOfServiceLevel(parameters.QualityOfServiceLevel)
                .WithRetainFlag(parameters.Retain)
                .Build();

            _serverEx.PublishAsync(message).GetAwaiter().GetResult();
            _outboundCounter.Increment();

            _logger.Log(LogLevel.Trace, $"Published MQTT topic '{parameters.Topic}.");
        }

        public string Subscribe(string uid, string topicFilter, Action<MqttApplicationMessageReceivedEventArgs> callback)
        {
            if (topicFilter == null) throw new ArgumentNullException(nameof(topicFilter));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (string.IsNullOrEmpty(uid))
            {
                uid = Guid.NewGuid().ToString("D");
            }

            lock (_subscribers)
            {
                _subscribers[uid] = new MqttSubscriber(uid, topicFilter, callback);
            }

            // Enqueue all retained messages to match the expected MQTT behavior.
            // Here we have no client per subscription. So we need to adopt some
            // features here manually.
            foreach (var retainedMessage in _serverEx.GetRetainedApplicationMessagesAsync().GetAwaiter().GetResult())
            {
                _incomingMessages.Add(new MqttApplicationMessageReceivedEventArgs(null, retainedMessage));
            }

            return uid;
        }

        public void Unsubscribe(string uid)
        {
            lock (_subscribers)
            {
                _subscribers.Remove(uid);
            }
        }

        public Task<IList<IMqttClientStatus>> GetClientsAsync()
        {
            return _serverEx.GetClientStatusAsync();
        }

        public Task<IList<IMqttSessionStatus>> GetSessionsAsync()
        {
            return _serverEx.GetSessionStatusAsync();
        }

        private void ProcessIncomingMqttMessages(CancellationToken cancellationToken)
        {
            Thread.CurrentThread.Name = nameof(ProcessIncomingMqttMessages);

            while (!cancellationToken.IsCancellationRequested)
            {
                MqttApplicationMessageReceivedEventArgs message = null;
                try
                {
                    message = _incomingMessages.Take(cancellationToken);
                    if (message == null || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var affectedSubscribers = new List<MqttSubscriber>();
                    lock (_subscribers)
                    {
                        foreach (var subscriber in _subscribers.Values)
                        {
                            if (subscriber.IsFilterMatch(message.ApplicationMessage.Topic))
                            {
                                affectedSubscribers.Add(subscriber);
                            }
                        }
                    }

                    foreach (var subscriber in affectedSubscribers)
                    {
                        TryNotifySubscriber(subscriber, message);
                    }

                    _outboundCounter.Increment();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception, $"Error while processing MQTT message with topic '{message?.ApplicationMessage?.Topic}'.");
                }
            }
        }

        private void TryNotifySubscriber(MqttSubscriber subscriber, MqttApplicationMessageReceivedEventArgs message)
        {
            try
            {
                subscriber.Notify(message);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Error while notifying subscriber '{subscriber.Uid}'.");
            }
        }
    }
}
