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

namespace IoTSharp.Hub
{

    public static class MqttExtension
    {
        public static void AddMqttClient(this IServiceCollection services, MqttClientSetting setting)
        {
            if (setting == null) setting = new MqttClientSetting();
            services.AddSingleton(options => new MQTTnet.MqttFactory().CreateMqttClient());
            services.AddTransient(options => new MqttClientOptionsBuilder()
                                     .WithClientId("buind-in")
                                     .WithTcpServer((setting.MqttBroker == "built-in" || string.IsNullOrEmpty(setting.MqttBroker)) ? "127.0.0.1" : setting.MqttBroker, setting.Port)
                                     .WithCredentials(setting.UserName, setting.Password)
                                     .WithCleanSession()
                                     .Build());
        }
        public static void UseIoTSharpMqttClient(this IApplicationBuilder app)
        {
            var _logger = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger<MqttClient>();
            var mqtt = app.ApplicationServices.GetService<IMqttClient>();
            var clientOptions = app.ApplicationServices.GetService<IMqttClientOptions>();
            mqtt.ApplicationMessageReceived += (sender, e) =>
                {
                    _logger.LogInformation($"Received  : {e.ApplicationMessage.Topic}");
                };
            mqtt.Connected += (sender, e) =>
                {
                    _logger.LogInformation($"CONNECTED  IsSessionPresent: {e.IsSessionPresent}");
                };
            mqtt.Disconnected += async (s, e) =>
            {
                _logger.LogInformation($"DISCONNECTED FROM SERVER  ClientWasConnected:{e.ClientWasConnected}, Exception={ e.Exception.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqtt.ConnectAsync(clientOptions);
                    _logger.LogInformation("RECONNECT AGAIN");
                }
                catch (Exception exception)
                {
                    _logger.LogError("CONNECTING FAILED", exception);
                }
            };

            try
            {
                Task.Run(() =>
                {
                    mqtt.ConnectAsync(clientOptions);
                    _logger.LogInformation("CONNECTED");
                });
            }
            catch (Exception exception)
            {
                _logger.LogError("CONNECTING FAILED", exception);
            }
        }
        public static void AddIoTSharpMqttServer( this IServiceCollection services,MqttBrokerSetting setting)
        {

            services.AddMqttTcpServerAdapter();
            services.AddHostedMqttServer(options =>
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
                options.WithConnectionValidator(action =>
                {
                    action.ReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted;
                });
                  options.Build();
            });
            services.AddMqttConnectionHandler();
            services.AddMqttWebSocketServerAdapter();
        }
        public static void UseIotSharpMqttServer(this IApplicationBuilder app)
        {
            app.UseMqttEndpoint();
            app.UseMqttServer(server =>
                {
                    server.ClientConnected += (sender, e) => { };
                    server.Started += (sender, e) => { };
                    server.Stopped += (sender, e) => { };
                    server.ApplicationMessageReceived += (sender, e) => { };
                    server.ClientSubscribedTopic += (sender, e) => { };
                    server.ClientUnsubscribedTopic += (sender, e) => { };
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


    }
}
