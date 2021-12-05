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
using MQTTnet.AspNetCoreEx;
using IoTSharp.Handlers;
using IoTSharp.Services;
using MQTTnet.Server;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MQTTnet.AspNetCore.Extensions;
using MQTTnet.Diagnostics.Logger;
using System.Security.Cryptography.X509Certificates;

namespace IoTSharp
{
    public static class MqttExtension
    {
        //static private IMqttServer _mqttServer;
        public static void AddIoTSharpMqttServer(this IServiceCollection services, MqttBrokerSetting broker)
        {
            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServerEx(options =>
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
            IMqttServerStorage storage = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IMqttServerStorage>();
            app.UseMqttServerEx(server =>
                {
                    server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(args => mqttEvents.Server_ClientConnected(server, args));
                    server.StartedHandler = new MqttServerStartedHandlerDelegate(args => mqttEvents.Server_Started(server, args));
                    server.StoppedHandler = new MqttServerStoppedHandlerDelegate(args => mqttEvents.Server_Stopped(server, args));
                    server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(args => mqttEvents.Server_ApplicationMessageReceived(server, args));
                    server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedTopicHandlerDelegate( args =>    mqttEvents.Server_ClientSubscribedTopic(server, args));
                    server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(args => mqttEvents.Server_ClientUnsubscribedTopic(server, args));
                    server.ClientConnectionValidatorHandler = new MqttServerClientConnectionValidatorHandlerDelegate(args => mqttEvents.Server_ClientConnectionValidator(server, args));
                    server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(args => mqttEvents.Server_ClientDisconnected(server, args));
                });
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
