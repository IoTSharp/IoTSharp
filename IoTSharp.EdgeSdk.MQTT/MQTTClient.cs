using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IoTSharp.EdgeSdk.MQTT
{
    public class MQTTClient
    {
        private readonly ILogger _logger;
        public MQTTClient()
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger(typeof(MQTTClient));
        }
        
        public MQTTClient(ILogger logger)
        {
            _logger = logger;
        }
        public MQTTClient(Uri  uri, ILogger logger)
        {
            _logger = logger;
            BrokerUri = uri;
        }
        public string DeviceId { get; set; } = string.Empty;
        public Uri BrokerUri { get; set; }
        public bool IsConnected => (Client?.IsConnected).GetValueOrDefault();
        private IMqttClient Client { get; set; }
        public event EventHandler<RpcRequest> OnExcRpc;
        public event EventHandler<AttributeResponse> OnReceiveAttributes;
        public Task<bool> ConnectAsync(Uri uri, string accesstoken) => ConnectAsync(uri, accesstoken, null);

        public async Task<bool> ConnectAsync(Uri uri, string username, string password)
        {
            if (BrokerUri == null && uri != null) BrokerUri = uri;
            if (BrokerUri != null && uri == null) uri = BrokerUri;
            bool initok = false;
            try
            {
                var factory = new MqttFactory();
                MqttNetLogger mqttNetLogger = new MqttNetLogger();
                mqttNetLogger.LogMessagePublished += MqttNetLogger_LogMessagePublished; ;
                Client = factory.CreateMqttClient(mqttNetLogger);
                var clientOptions = new MqttClientOptionsBuilder()
                       .WithClientId(uri.ToString() + Guid.NewGuid().ToString())
                          .WithTcpServer(uri.Host, uri.Port)
                        .WithCredentials(username, password)
                        .Build();
                Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(args => Client_ApplicationMessageReceived(Client, args));
                Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(args => Client_ConnectedAsync(Client, args));
                Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
                {
                    try
                    {
                        await Client.ConnectAsync(clientOptions);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError("CONNECTING FAILED", exception);
                    }
                });

                try
                {
                    var result = await Client.ConnectAsync(clientOptions);
                    initok = result.ResultCode == MqttClientConnectResultCode.Success;
                }
                catch (Exception exception)
                {
                    _logger.LogError("CONNECTING FAILED", exception);
                }
                _logger.LogInformation("WAITING FOR APPLICATION MESSAGES");
            }
            catch (Exception exception)
            {
                _logger.LogError("CONNECTING FAILED", exception);
            }
            return initok;
        }

        private void MqttNetLogger_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
        {
        }


        private   void Client_ConnectedAsync(object sender, MqttClientConnectedEventArgs e)
        {

            Client.SubscribeAsync($"/devices/{DeviceId}/rpc/request/+/+");
            Client.SubscribeAsync($"/devices/{DeviceId}/attributes/update/", MqttQualityOfServiceLevel.ExactlyOnce);
            _logger.LogInformation($"CONNECTED WITH SERVER ");

        }

        private void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogDebug($"ApplicationMessageReceived Topic {e.ApplicationMessage.Topic}  QualityOfServiceLevel:{e.ApplicationMessage.QualityOfServiceLevel} Retain:{e.ApplicationMessage.Retain} ");
            try
            {
                if (e.ApplicationMessage.Topic.StartsWith($"/devices/") && e.ApplicationMessage.Topic.Contains("/response/"))
                {
                    ReceiveAttributes(e);
                }
                else if (e.ApplicationMessage.Topic.StartsWith($"/devices/") && e.ApplicationMessage.Topic.Contains("/rpc/request/"))
                {
                    var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var rpcmethodname = tps[4];
                    var rpcdevicename = tps[1];
                    var rpcrequestid = tps[5];
                    _logger.LogInformation($"rpcmethodname={rpcmethodname} ");
                    _logger.LogInformation($"rpcdevicename={rpcdevicename } ");
                    _logger.LogInformation($"rpcrequestid={rpcrequestid}   ");
                    if (!string.IsNullOrEmpty(rpcmethodname) && !string.IsNullOrEmpty(rpcdevicename) && !string.IsNullOrEmpty(rpcrequestid))
                    {
                        OnExcRpc?.Invoke(Client, new RpcRequest()
                        {
                            Method = rpcmethodname,
                            DeviceId = rpcdevicename,
                            RequestId = rpcrequestid,
                            Params = e.ApplicationMessage.ConvertPayloadToString()
                        });
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic},Payload:{e.ApplicationMessage.ConvertPayloadToString()}", ex);
            }
        }

        private void ReceiveAttributes(MqttApplicationMessageReceivedEventArgs e)
        {
            var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var rpcmethodname = tps[2];
            var rpcdevicename = tps[1];
            var rpcrequestid = tps[4];
            _logger.LogInformation($"rpcmethodname={rpcmethodname} ");
            _logger.LogInformation($"rpcdevicename={rpcdevicename } ");
            _logger.LogInformation($"rpcrequestid={rpcrequestid}   ");

            if (!string.IsNullOrEmpty(rpcmethodname) && !string.IsNullOrEmpty(rpcdevicename) && !string.IsNullOrEmpty(rpcrequestid))
            {
                if (e.ApplicationMessage.Topic.Contains("/attributes/"))
                {
                    OnReceiveAttributes?.Invoke(Client, new AttributeResponse()
                    {
                        KeyName = rpcmethodname,
                        DeviceName = rpcdevicename,
                        Id = rpcrequestid,
                        Data = e.ApplicationMessage.ConvertPayloadToString()
                    });
                }
            }
        }

        public Task UploadAttributeAsync(object obj) => UploadAttributeAsync("me", obj);


        public Task UploadAttributeAsync(string _devicename, object obj)
        {
            return Client.PublishAsync($"devices/{_devicename}/attributes", Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }

        public Task UploadTelemetryDataAsync(object obj) => UploadTelemetryDataAsync("me", obj);

        public Task UploadTelemetryDataAsync(string _devicename, object obj)
        {
            return Client.PublishAsync($"devices/{_devicename}/telemetry", Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }

        public Task ResponseExecommand(RpcResponse rpcResult)
        {
            ///IoTSharp/Clients/RpcClient.cs#L65     var responseTopic = $"/devices/{deviceid}/rpc/response/{methodName}/{rpcid}";
            string topic = $"/devices/{rpcResult.DeviceId}/rpc/response/{rpcResult.Method.ToString()}/{rpcResult.ResponseId}";
            return Client.PublishAsync(topic, rpcResult.Data.ToString(), MqttQualityOfServiceLevel.ExactlyOnce);
        }
        public Task RequestAttributes(params string[] args) => RequestAttributes("me", false, args);
        public Task RequestAttributes(string _device, params string[] args) => RequestAttributes(_device, false, args);
        public Task RequestAttributes(bool anySide = true, params string[] args) => RequestAttributes("me", true, args);

        public Task RequestAttributes(string _device, bool anySide  , params string[] args)
        {
            string id = Guid.NewGuid().ToString();
            string topic = $"devices/{_device}/attributes/request/{id}";
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys.Add(anySide ? "anySide" : "server", string.Join(",", args));
            Client.SubscribeAsync($"/devices/{_device}/attributes/response/{id}", MqttQualityOfServiceLevel.ExactlyOnce);
            return Client.PublishAsync(topic, Newtonsoft.Json.JsonConvert.SerializeObject(keys), MqttQualityOfServiceLevel.ExactlyOnce);
        }
    }
    public class RpcRequest
    {
        public string DeviceId { get; set; }
        public string Method { get; set; }
        public string RequestId { get; set; }
        public string Params { get; set; }
    }
    public class RpcResponse
    {
        public string DeviceId { get; set; }
        public string Method { get; set; }
        public string ResponseId { get; set; }
        public string Data { get; set; }
    }
    public class AttributeResponse
    {
        public string Id { get; set; }
        public string DeviceName { get; set; }
        public string KeyName { get; set; }
   
        public string Data { get; set; }
    }
    
}

