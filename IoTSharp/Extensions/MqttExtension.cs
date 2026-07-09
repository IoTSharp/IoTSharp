using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using MQTTnet.AspNetCore;
using IoTSharp.Services;
using IoTSharp.Services.MQTTControllers;
using MQTTnet.Server;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using MQTTnet.AspNetCore.Routing;
using IoTSharp.Data;
using IoTSharp.Extensions;
using IoTSharp.Contracts;
using System.Net.Security;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp
{
    public static class MqttExtension
    {
        public const string ServerSenderClientId = "iotsharp-server";
        private const string JsonContentType = "application/json";
        private const string TextContentType = "text/plain; charset=utf-8";
        private const string BinaryContentType = "application/octet-stream";
        private static readonly Type[] MqttControllerTypes = new[]
        {
            typeof(AlarmController),
            typeof(AttributesController),
            typeof(DataController),
            typeof(GatewayController),
            typeof(RpcController),
            typeof(TelemetryController),
            typeof(V1GatewayController)
        };

        public static void AddIoTSharpMqttServer(this IServiceCollection services, MqttBrokerSetting broker)
        {
            services.AddMqttControllers(MqttControllerTypes, options =>
            {
                options.WithJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                options.WithDefaultPayloadContentType(JsonContentType);
                options.WithDefaultPayloadFormatter("json");
                options.WithCaseSensitiveTopicMatching();
            });
            services.AddSingleton<MqttServer>();
            services.AddSingleton<MQTTService>();
            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServerWithServices(options =>
            {
                options.WithDefaultEndpointPort(broker.Port).WithDefaultEndpoint();
                if (broker.EnableTls)
                {
                    if (broker.CACertificate != null)
                    {
                        broker.CACertificate.LoadCAToRoot();

                    }
                    options.WithEncryptedEndpoint();
                    options.WithEncryptedEndpointPort(broker.TlsPort);
                    if (broker.BrokerCertificate != null)
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
            var mqttEvents = app.ApplicationServices.GetRequiredService<MQTTService>();
            app.UseMqttServer(server =>
                {
                    server.WithAttributeRouting(app.ApplicationServices, allowUnmatchedRoutes: true);
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
            return JsonNodeParser.ParseNode(msg.ConvertPayloadToString() ?? "{}")?.ToDictionary();
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
                    kv = new(item.KeyName, JsonObjectSerializer.DeserializeUntyped(item.Value_Json));
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

        /// <summary>
        /// 解析 MQTT v5 response topic；旧客户端未提供时回退到 IoTSharp 既有响应 topic。
        /// </summary>
        /// <param name="requestContext">当前 MQTT 请求上下文。</param>
        /// <param name="fallbackTopic">兼容 MQTT 3.x 或旧协议的响应 topic。</param>
        /// <returns>最终使用的响应 topic。</returns>
        public static string ResolveResponseTopic(this MqttRequestContext requestContext, string fallbackTopic)
        {
            if (!string.IsNullOrWhiteSpace(requestContext?.ResponseTopic))
            {
                return requestContext.ResponseTopic;
            }

            return fallbackTopic;
        }

        /// <summary>
        /// 通过 MQTTnet server 注入 JSON 响应消息，按 broker 订阅关系投递。
        /// </summary>
        /// <typeparam name="T">响应 payload 类型。</typeparam>
        /// <param name="mqtt">MQTT server。</param>
        /// <param name="senderClientId">服务端注入消息使用的发送方 client id。</param>
        /// <param name="topic">响应 topic。</param>
        /// <param name="payload">响应对象。</param>
        /// <param name="requestContext">可选请求上下文，用于沿用 QoS、correlation data 等 MQTT v5 元数据。</param>
        /// <param name="contentType">响应 content type。</param>
        public static async Task PublishAsync<T>(this MqttServer mqtt, string senderClientId, string topic, T payload, MqttRequestContext requestContext = null, string contentType = JsonContentType) where T : class
        {
            var message = BuildApplicationMessage(topic, JsonSerializer.SerializeToUtf8Bytes(payload), requestContext, contentType);
            await mqtt.PublishAsync(senderClientId, message);
        }

        /// <summary>
        /// 通过 MQTTnet server 注入文本响应消息，按 broker 订阅关系投递。
        /// </summary>
        /// <param name="mqtt">MQTT server。</param>
        /// <param name="senderClientId">服务端注入消息使用的发送方 client id。</param>
        /// <param name="topic">响应 topic。</param>
        /// <param name="payload">响应文本。</param>
        /// <param name="requestContext">可选请求上下文，用于沿用 QoS、correlation data 等 MQTT v5 元数据。</param>
        /// <param name="contentType">响应 content type。</param>
        public static async Task PublishAsync(this MqttServer mqtt, string senderClientId, string topic, string payload, MqttRequestContext requestContext = null, string contentType = TextContentType)
        {
            var message = BuildApplicationMessage(topic, Encoding.UTF8.GetBytes(payload ?? string.Empty), requestContext, contentType);
            await mqtt.PublishAsync(senderClientId, message);
        }

        /// <summary>
        /// 通过 MQTTnet server 注入二进制响应消息，按 broker 订阅关系投递。
        /// </summary>
        /// <param name="mqtt">MQTT server。</param>
        /// <param name="senderClientId">服务端注入消息使用的发送方 client id。</param>
        /// <param name="topic">响应 topic。</param>
        /// <param name="payload">响应二进制内容。</param>
        /// <param name="requestContext">可选请求上下文，用于沿用 QoS、correlation data 等 MQTT v5 元数据。</param>
        /// <param name="contentType">响应 content type。</param>
        public static async Task PublishAsync(this MqttServer mqtt, string senderClientId, string topic, byte[] payload, MqttRequestContext requestContext = null, string contentType = BinaryContentType)
        {
            var message = BuildApplicationMessage(topic, payload ?? Array.Empty<byte>(), requestContext, contentType);
            await mqtt.PublishAsync(senderClientId, message);
        }

        /// <summary>
        /// 通过 MQTTnet server 官方注入入口发布消息，避免直接操作客户端 session 队列。
        /// </summary>
        /// <param name="mqtt">MQTT server。</param>
        /// <param name="senderClientId">服务端注入消息使用的发送方 client id。</param>
        /// <param name="message">要注入 broker 的消息。</param>
        public static async Task PublishAsync(this MqttServer mqtt, string senderClientId, MqttApplicationMessage message)
        {
            if (mqtt == null)
            {
                throw new ArgumentNullException(nameof(mqtt));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var injectedMessage = new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = string.IsNullOrWhiteSpace(senderClientId) ? ServerSenderClientId : senderClientId,
                SenderUserName = ServerSenderClientId
            };
            await mqtt.InjectApplicationMessage(injectedMessage);
        }

        private static MqttApplicationMessage BuildApplicationMessage(
            string topic,
            byte[] payload,
            MqttRequestContext requestContext,
            string contentType)
        {
            var builder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload ?? Array.Empty<byte>())
                .WithQualityOfServiceLevel(requestContext?.QualityOfServiceLevel ?? MqttQualityOfServiceLevel.AtMostOnce);

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                builder.WithContentType(contentType);
            }

            if (requestContext?.CorrelationData is { Length: > 0 } correlationData)
            {
                builder.WithCorrelationData(correlationData);
            }

            return builder.Build();
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
