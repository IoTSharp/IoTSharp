﻿using IoTSharp.Extensions.X509;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.EdgeSdk.MQTT
{
    public class MQTTClient
    {
 
        public string DeviceId { get; set; } = string.Empty;
 
        public bool IsConnected => (Client?.IsConnected).GetValueOrDefault();
        private IMqttClient Client { get; set; }
        public delegate void DLogError(string message,Exception exception );
        public event DLogError LogError;
        public delegate void DLogInformation(string message);
        public event DLogInformation LogInformation;
        public delegate void DLogDebug(string message);
        public event DLogDebug LogDebug;

        public event EventHandler<RpcRequest> OnExcRpc;
      
        public event EventHandler<AttributeResponse> OnReceiveAttributes;

        public async Task<bool> ConnectAsync(FileInfo x509_zipfile)
        {
            if (x509_zipfile.Exists)
            {
                var file = System.IO.Compression.ZipFile.OpenRead(x509_zipfile.FullName);
                byte[] getzipfile(string filename)
                {
                    byte[] unzippedArray = null;
                    using (var unzippedEntryStream = file.GetEntry(filename).Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            unzippedEntryStream.CopyTo(ms);
                            unzippedArray = ms.ToArray();
                        }
                    }
                    return unzippedArray;
                };
                X509Certificate2 ca ;
                X509Certificate2 client;
                try
                {
                    ca = X509Certificate2.CreateFromPem(System.Text.Encoding.Default.GetChars(getzipfile("ca.crt")));
                    var pemx509 = X509Certificate2.CreateFromPem(System.Text.Encoding.Default.GetChars(getzipfile("client.crt")), System.Text.Encoding.Default.GetChars(getzipfile("client.key")));
                    client = pemx509.ToPkcs12();
                }
                catch (Exception ex)
                {
                    throw new Exception("从Zip中加载证书文件错误", ex);
                }
                if (ca!=null && client!=null)
                {
                    return await ConnectAsync(ca, client);
                }
                else
                {
                    throw new Exception("证书无效");
                }
            }
            else
            {
                throw new Exception("指定的ZIP文件不存在");
            }
        }



        public async Task<bool> ConnectAsync(X509Certificate2 ca, X509Certificate2 client, Uri uri=null)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTlsOptions(
                    new MqttClientTlsOptions ()
                    {
                        UseTls = true,
                        SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                         AllowUntrustedCertificates = true,
                          
                        CertificateValidationHandler = (certContext) =>
                        {
                            var chain = certContext.Chain;
                            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                            chain.ChainPolicy.VerificationTime = DateTime.UtcNow;
                            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
                            chain.ChainPolicy.CustomTrustStore.Add(ca);
                            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                            var x5092 = new X509Certificate2(certContext.Certificate);
                            return chain.Build(x5092);
                        }, 
                         CertificateSelectionHandler = (certContext) =>
                         {
                             return client;
                         }
                        
                   
                    });
            if (uri != null)
            {
                options.WithTcpServer(uri.Host, uri.Port);
            }
            else
            {
                if (Uri.TryCreate(client.GetNameInfo(X509NameType.UrlName, false), UriKind.RelativeOrAbsolute, out Uri _uri))
                {
                    options.WithTcpServer(_uri.Host, _uri.Port);
                }
                else
                {
                    var host = client.GetNameInfo(X509NameType.DnsName, false);
                    options.WithTcpServer(host, 8883);
                }
            }
       
            return await ConnectAsync(options.Build());
        }
     
 
        public Task<bool> ConnectAsync(Uri uri, string accesstoken) => ConnectAsync(uri, accesstoken, null);

       
        public async Task<bool> ConnectAsync(Uri uri, string username, string password)
        {
            if (uri == null) throw new ArgumentNullException("url");
            var clientOptions = new MqttClientOptionsBuilder()
                   .WithClientId(uri.ToString() + Guid.NewGuid().ToString())
                      .WithTcpServer(uri.Host, uri.Port)
                    .WithCredentials(username, password)
                    .Build();
           return  await  ConnectAsync(clientOptions);
        }

   
        public async Task<bool> ConnectAsync(MqttClientOptions clientOptions, System.Threading.CancellationToken cancellationToken=default)
        {
            bool initok = false;
            try
            {
                var factory = new MqttClientFactory();
                Client = factory.CreateMqttClient( );
                Client.ApplicationMessageReceivedAsync +=  Client_ApplicationMessageReceived;
                Client.ConnectedAsync += e => {
                    Client.SubscribeAsync($"devices/{DeviceId}/rpc/request/+/+");
                    Client.SubscribeAsync($"devices/{DeviceId}/attributes/update/", MqttQualityOfServiceLevel.ExactlyOnce);
                    LogInformation?.Invoke($"CONNECTED WITH SERVER ");
                    return Task.CompletedTask;
                };
         
                try
                {
                    var result = await Client.ConnectAsync(clientOptions, cancellationToken);
                    initok = result.ResultCode == MqttClientConnectResultCode.Success;
                }
                catch (Exception exception)
                {
                    LogError?.Invoke("CONNECTING FAILED", exception);
                }
                LogInformation?.Invoke("WAITING FOR APPLICATION MESSAGES");
            }
            catch (Exception exception)
            {
                 LogError?.Invoke("CONNECTING FAILED", exception);
            }
            return initok;
        }

       

    

        private Task Client_ApplicationMessageReceived( MqttApplicationMessageReceivedEventArgs e)
        {
           LogDebug?.Invoke($"ApplicationMessageReceived Topic {e.ApplicationMessage.Topic}  QualityOfServiceLevel:{e.ApplicationMessage.QualityOfServiceLevel} Retain:{e.ApplicationMessage.Retain} ");
            try
            {
                if (e.ApplicationMessage.Topic.StartsWith($"devices/") && e.ApplicationMessage.Topic.Contains("/response/"))
                {
                    ReceiveAttributes(e);
                }
                else if (e.ApplicationMessage.Topic.StartsWith($"devices/") && e.ApplicationMessage.Topic.Contains("/rpc/request/"))
                {
                    var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var rpcmethodname = tps[4];
                    var rpcdevicename = tps[1];
                    var rpcrequestid = tps[5];
                    LogInformation?.Invoke($"rpcmethodname={rpcmethodname} ");
                     LogInformation?.Invoke($"rpcdevicename={rpcdevicename } ");
                    LogInformation?.Invoke($"rpcrequestid={rpcrequestid}   ");
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
                LogError?.Invoke($"ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic},Payload:{e.ApplicationMessage.ConvertPayloadToString()}", ex);
            }
            return Task.CompletedTask;
        }

        private void ReceiveAttributes(MqttApplicationMessageReceivedEventArgs e)
        {
            var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var rpcmethodname = tps[2];
            var rpcdevicename = tps[1];
            var rpcrequestid = tps[4];
             LogInformation?.Invoke($"rpcmethodname={rpcmethodname} ");
             LogInformation?.Invoke($"rpcdevicename={rpcdevicename } ");
             LogInformation?.Invoke($"rpcrequestid={rpcrequestid}   ");

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
            return Client.PublishAsync(new MqttApplicationMessageBuilder().WithTopic(  $"devices/{_devicename}/attributes").WithPayload( Newtonsoft.Json.JsonConvert.SerializeObject(obj)).Build());
        }

        public Task UploadTelemetryDataAsync(object obj) => UploadTelemetryDataAsync("me", obj);

        public Task UploadTelemetryDataAsync(string _devicename, object obj)
        {
            return Client.PublishAsync(new MqttApplicationMessageBuilder().WithTopic($"devices/{_devicename}/telemetry").WithPayload(Newtonsoft.Json.JsonConvert.SerializeObject(obj)).Build());
        }

        public Task ResponseExecommand(RpcResponse rpcResult)
        {
            ///IoTSharp/Clients/RpcClient.cs#L65     var responseTopic = $"/devices/{deviceid}/rpc/response/{methodName}/{rpcid}";
            string topic = $"devices/{rpcResult.DeviceId}/rpc/response/{rpcResult.Method.ToString()}/{rpcResult.ResponseId}";
            return Client.PublishAsync(new MqttApplicationMessageBuilder().WithTopic( topic).WithPayload( rpcResult.Data.ToString()).WithQualityOfServiceLevel( MqttQualityOfServiceLevel.ExactlyOnce).Build());
        }
        public Task RequestAttributes(params string[] args) => RequestAttributes("me", args);

        public Task RequestAttributes(string _device, params string[] args)
        {
            string id = Guid.NewGuid().ToString();
            string topic = $"devices/{_device}/attributes/request/{id}";
            Client.SubscribeAsync($"devices/{_device}/attributes/response/{id}", MqttQualityOfServiceLevel.ExactlyOnce);
            return Client.PublishStringAsync(topic, Newtonsoft.Json.JsonConvert.SerializeObject(args), MqttQualityOfServiceLevel.ExactlyOnce);
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

