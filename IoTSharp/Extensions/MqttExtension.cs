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

namespace IoTSharp
{
    public static class MqttExtension
    {
        public static void AddIoTSharpMqttServer(this IServiceCollection services, MqttBrokerSetting setting)
        {

            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServerEx(options =>
            {
                var broker = setting;
                if (broker == null) broker = new MqttBrokerSetting();
                options.WithDefaultEndpointPort(broker.Port).WithDefaultEndpointBoundIPAddress(System.Net.IPAddress.Parse("127.0.0.1")).WithDefaultEndpoint();
                if (broker.EnableTls)
                {
                    options.WithEncryptedEndpoint();
                    options.WithEncryptedEndpointPort(broker.TlsPort);
                    if (System.IO.File.Exists(broker.Certificate))
                    {
                        options.WithEncryptionCertificate(System.IO.File.ReadAllBytes(broker.Certificate)).WithEncryptionSslProtocol(broker.SslProtocol);
                    }
                }
                else
                {
                    options.WithoutEncryptedEndpoint();
                }
                options.Build();
            });
            services.AddMqttConnectionHandler();
            services.AddMqttWebSocketServerAdapter();
            services.AddTransient<MqttEventsHandler>();
        }
        public static void UseIotSharpMqttServer(this IApplicationBuilder app)
        {
            app.UseMqttEndpoint();
            var mqttEvents = app.ApplicationServices.CreateScope().ServiceProvider.GetService<MqttEventsHandler>();
            app.UseMqttServerEx(server =>
                {
                    server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(args => mqttEvents.Server_ClientConnected(server, args));
                    server.StartedHandler = new MqttServerStartedHandlerDelegate(args => mqttEvents.Server_Started(server, args));
                    server.StoppedHandler = new MqttServerStoppedHandlerDelegate(args => mqttEvents.Server_Stopped(server, args));
                    server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(args => mqttEvents.Server_ApplicationMessageReceived(server, args));
                    server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(args => mqttEvents.Server_ClientSubscribedTopic(server, args));
                    server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(args => mqttEvents.Server_ClientUnsubscribedTopic(server, args));
                    server.ClientConnectionValidatorHandler = new MqttServerClientConnectionValidatorHandlerDelegate(args => mqttEvents.Server_ClientConnectionValidator(server, args));
                    server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(args => mqttEvents.Server_ClientDisconnected(server, args));
 
                });

            var mqttNetLogger = app.ApplicationServices.GetService<IMqttNetLogger>();
            var _loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = _loggerFactory.CreateLogger<IMqttNetLogger>();
            mqttNetLogger.LogMessagePublished += (object sender, MqttNetLogMessagePublishedEventArgs e) =>
            {
                var message = $"ID:{e.TraceMessage.LogId},ThreadId:{e.TraceMessage.ThreadId},Source:{e.TraceMessage.Source},Timestamp:{e.TraceMessage.Timestamp},Message:{e.TraceMessage.Message}";
                switch (e.TraceMessage.Level)
                {
                    case MqttNetLogLevel.Verbose:
                        logger.LogTrace(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Info:
                        logger.LogInformation(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Warning:
                        logger.LogWarning(e.TraceMessage.Exception, message);
                        break;

                    case MqttNetLogLevel.Error:
                        logger.LogError(e.TraceMessage.Exception, message);
                        break;

                    default:
                        break;
                }
            };
        }

        public static void AddMqttClient(this IServiceCollection services, MqttClientSetting setting)
        {
            if (setting == null) setting = new MqttClientSetting();
            services.AddSingleton(options => new MQTTnet.MqttFactory().CreateMqttClient());
            services.AddTransient(options => new MqttClientOptionsBuilder()
                                     .WithClientId("buind-in")
                                     .WithTcpServer((setting.MqttBroker == "built-in" || string.IsNullOrEmpty(setting.MqttBroker)) ? "127.0.0.1" : setting.MqttBroker, setting.Port)
                                     .WithCredentials(setting.UserName, setting.Password)
                                     .WithCleanSession()//.WithProtocolVersion (MQTTnet.Formatter.MqttProtocolVersion.V500)
                                     .Build());
            services.AddHostedService <MqttClientService>();
        }

    }
}
