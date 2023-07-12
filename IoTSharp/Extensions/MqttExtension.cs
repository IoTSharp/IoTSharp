using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using MQTTnet.AspNetCore;
using MQTTnet.Diagnostics;
using IoTSharp.Services;
using MQTTnet.Server;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using MQTTnet.AspNetCore.Routing;
using IoTSharp.Data;
using Newtonsoft.Json.Linq;
using IoTSharp.Contracts;
using System.Net.Security;
using System.Runtime.ConstrainedExecution;

namespace IoTSharp
{
    public static class MqttExtension
    {

        public static void AddIoTSharpMqttServer(this IServiceCollection services, MqttBrokerSetting broker)
        {
            services.AddMqttControllers();
            services.AddSingleton<MqttServer>();
            services.AddSingleton<MQTTService>();
            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServerWithServices(options =>
            {
                options.WithDefaultEndpointPort(broker.Port).WithDefaultEndpoint();
                if (broker.EnableTls)
                {
                    if (broker.CACertificate!=null)
                    {
                        broker.CACertificate.LoadCAToRoot();
                   
                    }
                    options.WithEncryptedEndpoint();
                    options.WithEncryptedEndpointPort(broker.TlsPort);
                    if (broker.BrokerCertificate!=null)
                    {
                        options.WithEncryptionCertificate(broker.CACertificate.Export(X509ContentType.Pfx)).WithEncryptionSslProtocol(broker.SslProtocol);
                    }
                    options.WithClientCertificate((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                    {
                        bool result = false;
                        try
                        {
                            //如果CA跟证书是受信任， 这里就是None。
                            if (sslPolicyErrors == SslPolicyErrors.None)
                            {
                                result = true;
                            }
                            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors
                                && chain.ChainStatus.Count() == 1 && chain.ChainStatus.First().Status == X509ChainStatusFlags.UntrustedRoot)
                            {
                                //如果有是远程证书链有问题， 并且只有 UntrustedRoot 时，内部开始验证客户端是不是由本机CA证书颁发的
                                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                                chain.ChainPolicy.CustomTrustStore.Add(broker.CACertificate);
                                if (chain.Build((X509Certificate2)certificate))//如果是本CA办法， 则能构建成功， 如果是其他CA办法，则失败。 
                                {
                                    //确认跟证书在当前
                                    result = chain.ChainElements.Cast<X509ChainElement>().Any(a => a.Certificate.Thumbprint == broker.CACertificate.Thumbprint);
                                }
                            }
                        }
                        catch { }

                        return result;
                    });


                }
                else
                {
                    options.WithoutEncryptedEndpoint();
                }
                options.WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(5));
                options.WithPersistentSessions();
                options.Build();
            }).AddMqttConnectionHandler()
                    .AddConnections(); 
            services.AddMqttWebSocketServerAdapter();
        }

        public static void UseIotSharpMqttServer(this IApplicationBuilder app)
        {
            var mqttEvents = app.ApplicationServices.CreateScope().ServiceProvider.GetService<MQTTService>();
            app.UseAttributeRouting(true);
            app.UseMqttServer(server =>
                {
                    server.ClientConnectedAsync += mqttEvents.Server_ClientConnectedAsync;
                    server.StartedAsync += mqttEvents.Server_Started;
                    server.StoppedAsync += mqttEvents.Server_Stopped;
                    server.ClientSubscribedTopicAsync += mqttEvents.Server_ClientSubscribedTopic;
                    server.ClientUnsubscribedTopicAsync += mqttEvents.Server_ClientUnsubscribedTopic;
                    server.ValidatingConnectionAsync += mqttEvents.Server_ClientConnectionValidator;
                    server.ClientDisconnectedAsync += mqttEvents.Server_ClientDisconnected;
                });
        }



        public static Dictionary<string, object> ConvertPayloadToDictionary(this MqttApplicationMessage msg)
        {
            return JToken.Parse(msg.ConvertPayloadToString() ?? "{}")?.JsonToDictionary();
        }

        public static List<T> ConvertPayloadToList<T>(this MqttApplicationMessage msg)
        {
            var str = msg.ConvertPayloadToString() ?? "[]";
            return System.Text.Json.JsonSerializer.Deserialize<List<T>>(str);
        }

        public static KeyValuePair<string, object> AttributeToKeyValue(this AttributeLatest item)
        {
            KeyValuePair<string, object> kv;
            switch (item.Type)
            {
                case DataType.Boolean:
                    kv = new(item.KeyName, item.Value_Boolean);
                    break;
                case DataType.String:
                    kv = new(item.KeyName, item.Value_String);
                    break;

                case DataType.Long:
                    kv = new(item.KeyName, item.Value_Long);
                    break;

                case DataType.Double:
                    kv = new(item.KeyName, item.Value_Double);
                    break;

                case DataType.Json:
                    kv = new(item.KeyName, Newtonsoft.Json.Linq.JToken.Parse(item.Value_Json));
                    break;

                case DataType.XML:
                    kv = new(item.KeyName, item.Value_XML);
                    break;

                case DataType.Binary:
                    kv = new(item.KeyName, item.Value_Binary);
                    break;

                case DataType.DateTime:
                    kv = new(item.KeyName, item.Value_DateTime);
                    break;

                default:
                    kv = new(item.KeyName, item.Value_Json);
                    break;
            }
            return kv;
        }
        public static async Task PublishAsync<T>(this MqttServer mqtt, string SenderClientId, string topic, T _payload) where T : class
        {
            await mqtt.PublishAsync(SenderClientId, new MqttApplicationMessage() { Topic = topic, Payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_payload) });
        }
        public static async Task PublishAsync(this MqttServer mqtt, string SenderClientId, string topic, string _payload)
        {
            await mqtt.PublishAsync(SenderClientId, new MqttApplicationMessage() { Topic = topic, Payload = System.Text.Encoding.Default.GetBytes(_payload) });
        }
        public static async Task PublishAsync(this MqttServer mqtt, string SenderClientId, string topic, byte[] _payload)
        {
            await mqtt.PublishAsync(SenderClientId, new MqttApplicationMessage() { Topic = topic, Payload = _payload });
        }

        public static async Task   PublishAsync ( this MqttServer mqtt, string SenderClientId ,MqttApplicationMessage message)
        {
            var clients = await mqtt.GetClientsAsync();
            var client= clients.FirstOrDefault(c => c.Id == SenderClientId);
            await client.Session.EnqueueApplicationMessageAsync(message);
        }


        public static void AddMqttClient(this IServiceCollection services, MqttClientSetting setting)
        {
            services.AddTransient(options => new MqttClientOptionsBuilder()
                                     .WithClientId(setting.MqttBroker)
                                     .WithTcpServer((setting.MqttBroker == "built-in" || string.IsNullOrEmpty(setting.MqttBroker)) ? "127.0.0.1" : setting.MqttBroker, setting.Port)
                                     .WithCredentials(setting.UserName, setting.Password)
                                     .WithCleanSession()
                                     .Build());
        }
    }
}
