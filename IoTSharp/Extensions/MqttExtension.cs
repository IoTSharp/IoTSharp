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
using IoTSharp.Handlers;
using IoTSharp.Services;
using MQTTnet.Server;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;

namespace IoTSharp
{
    public static class MqttExtension
    {
        //static private IMqttServer _mqttServer;
        public static void AddIoTSharpMqttServer(this IServiceCollection services, MqttBrokerSetting broker)
        {
            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServer(options =>
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
                        options.WithEncryptionCertificate(broker.BrokerCertificate.Export(X509ContentType.Pfx)).WithEncryptionSslProtocol(broker.SslProtocol);
                    }
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
            services.AddSingleton<MQTTServerHandler>();
        }
        public static void UseIotSharpMqttServer(this IApplicationBuilder app)
        {
            var mqttEvents = app.ApplicationServices.CreateScope().ServiceProvider.GetService<MQTTServerHandler>();
            app.UseMqttServer(server =>
                {
                    server.ClientConnectedAsync +=  mqttEvents.Server_ClientConnectedAsync;
                    server.StartedAsync += mqttEvents.Server_Started ;
                    server.StoppedAsync +=  mqttEvents.Server_Stopped ;
                    server.ApplicationMessageNotConsumedAsync +=  mqttEvents.Server_ApplicationMessageReceived ;
                    server .ClientSubscribedTopicAsync    += mqttEvents.Server_ClientSubscribedTopic;
                    server.ClientUnsubscribedTopicAsync += mqttEvents.Server_ClientUnsubscribedTopic;
                    server.ValidatingConnectionAsync += mqttEvents.Server_ClientConnectionValidator;
                    server.ClientDisconnectedAsync    +=mqttEvents.Server_ClientDisconnected;
                });
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
