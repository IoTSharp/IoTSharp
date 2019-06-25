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
using IoTSharp.MQTT;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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
                options.WithPersistentSessions();
                options.Build();
            });
            services.AddMqttConnectionHandler();
            services.AddMqttWebSocketServerAdapter();
            services.AddSingleton<MQTTServerHandler>();
        }
        public static void UseIotSharpMqttServer(this IApplicationBuilder app)
        {
            app.UseMqttEndpoint();
            var mqttEvents = app.ApplicationServices.CreateScope().ServiceProvider.GetService<MQTTServerHandler>();
            IMqttServerStorage storage = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IMqttServerStorage>();
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
          
            services.AddSingleton(options => new MQTTnet.MqttFactory().CreateMqttClient());
            services.AddTransient(options => new MqttClientOptionsBuilder()
                                     .WithClientId(setting.MqttBroker)
                                     .WithTcpServer((setting.MqttBroker == "built-in" || string.IsNullOrEmpty(setting.MqttBroker)) ? "127.0.0.1" : setting.MqttBroker, setting.Port)
                                     .WithCredentials(setting.UserName, setting.Password)
                                     .WithCleanSession()//.WithProtocolVersion (MQTTnet.Formatter.MqttProtocolVersion.V500)
                                     .Build());
            services.AddHostedService<MqttClientService>();
        }

        public static void UseIotSharpSelfCollecting(this IApplicationBuilder app)
        {
            var _systemStatusService = app.ApplicationServices.CreateScope().ServiceProvider.GetService<RuntimeStatusHandler>();
            var _creationTimestamp = DateTime.Now;
            _systemStatusService.Set("startup.timestamp", _creationTimestamp);
            _systemStatusService.Set("framework.description", RuntimeInformation.FrameworkDescription);
            _systemStatusService.Set("process.architecture", RuntimeInformation.ProcessArchitecture);
            _systemStatusService.Set("process.id", Process.GetCurrentProcess().Id);
            _systemStatusService.Set("system.processor_count", Environment.ProcessorCount);
            _systemStatusService.Set("system.working_set", () => Environment.WorkingSet);
            _systemStatusService.Set("arguments", string.Join(" ", Environment.GetCommandLineArgs()));
            _systemStatusService.Set("iotsharp.version", typeof(Startup).Assembly.GetName().Version.ToString());
            _systemStatusService.Set("startup.duration", DateTime.Now - _creationTimestamp);
            _systemStatusService.Set("system.date_time", () => DateTime.Now);
            _systemStatusService.Set("up_time", () => DateTime.Now - _creationTimestamp);

            _systemStatusService.Set("os.description", RuntimeInformation.OSDescription);
            _systemStatusService.Set("os.architecture", RuntimeInformation.OSArchitecture);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _systemStatusService.Set("os.platform", "linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _systemStatusService.Set("os.platform", "windows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _systemStatusService.Set("os.platform", "osx");
            }

            _systemStatusService.Set("thread_pool.max_worker_threads", () =>
            {
                ThreadPool.GetMaxThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.max_completion_port_threads", () =>
            {
                ThreadPool.GetMaxThreads(out _, out var x);
                return x;
            });

            _systemStatusService.Set("thread_pool.min_worker_threads", () =>
            {
                ThreadPool.GetMinThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.min_completion_port_threads", () =>
            {
                ThreadPool.GetMinThreads(out _, out var x);
                return x;
            });

            _systemStatusService.Set("thread_pool.available_worker_threads", () =>
            {
                ThreadPool.GetAvailableThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.available_completion_port_threads", () =>
            {
                ThreadPool.GetAvailableThreads(out _, out var x);
                return x;
            });
        }

    }
}
