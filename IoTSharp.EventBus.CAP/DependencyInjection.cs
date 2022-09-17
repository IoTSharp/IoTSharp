
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Savorboard.CAP.InMemoryMessageQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using static IoTSharp.EventBus.EventBusOption;

namespace IoTSharp.EventBus.CAP
{
    public static class DependencyInjection
    {

        public static IApplicationBuilder UseCAPEventBus(this IApplicationBuilder app)
        {
            var provider = app.ApplicationServices;
            var options = provider.GetService<EventBusOption>();
            app.UseCapDashboard();
            return app;
        }

        public static void UserCAP(this EventBusOption  opt)
        {
            var settings = opt.AppSettings;
            var healthChecks = opt.HealthChecks;
            var _EventBusStore = opt.EventBusStore;
            var _EventBusMQ = opt.EventBusMQ;
            var services = opt.services;
            services.AddTransient<ISubscriber, CapSubscriber>();
            services.AddTransient<IPublisher, CapPublisher>();
            services.AddCap(x =>
            {
                string _hc_EventBusStore = $"{nameof(EventBusStore)}-{Enum.GetName(settings.EventBusStore)}";
                x.SucceedMessageExpiredAfter = settings.SucceedMessageExpiredAfter;
                x.ConsumerThreadCount = settings.ConsumerThreadCount;
                switch (settings.EventBusStore)
                {
                    case EventBusStore.PostgreSql:
                        x.UsePostgreSql(_EventBusStore);
                        healthChecks.AddNpgSql(_EventBusStore, name: _hc_EventBusStore);
                        break;

                    case EventBusStore.MongoDB:
                        x.UseMongoDB(_EventBusStore);  //注意，仅支持MongoDB 4.0+集群
                        healthChecks.AddMongoDb(_EventBusStore, name: _hc_EventBusStore);
                        break;

                    case EventBusStore.LiteDB:
                        x.UseLiteDBStorage(_EventBusStore);
                        break;
                    case EventBusStore.MySql:
                        x.UseMySql(_EventBusStore);
                        break;
                    case EventBusStore.SqlServer:
                        x.UseSqlServer(_EventBusStore);
                        break;
                    case EventBusStore.InMemory:
                    default:
                        x.UseInMemoryStorage();
                        break;
                }
                string _hc_EventBusMQ = $"{nameof(EventBusMQ)}-{Enum.GetName(settings.EventBusMQ)}";
                switch (settings.EventBusMQ)
                {
                    case EventBusMQ.RabbitMQ:
                        var url = new Uri(_EventBusMQ);
                        x.UseRabbitMQ(cfg =>
                        {
                            cfg.ConnectionFactoryOptions = cf =>
                            {
                                cf.AutomaticRecoveryEnabled = true;
                                cf.Uri = new Uri(_EventBusMQ);
                            };
                        });
                        //amqp://guest:guest@localhost:5672
                        healthChecks.AddRabbitMQ(connectionFactory =>
                        {
                            var factory = new ConnectionFactory()
                            {
                                Uri = new Uri(_EventBusMQ),
                                AutomaticRecoveryEnabled = true
                            };
                            return factory.CreateConnection();
                        }, _hc_EventBusMQ);
                        break;

                    case EventBusMQ.Kafka:
                        x.UseKafka(_EventBusMQ);
                        healthChecks.AddKafka(cfg =>
                        {
                            cfg.BootstrapServers = _EventBusMQ;
                        }, name: _hc_EventBusMQ);
                        break;

                    case EventBusMQ.ZeroMQ:
                        x.UseZeroMQ(cfg =>
                        {
                            cfg.HostName = _EventBusMQ ?? "127.0.0.1";
                            cfg.Pattern = MaiKeBing.CAP.NetMQPattern.PushPull;
                        });
                        break;
                    case EventBusMQ.AzureServiceBus:
                        x.UseAzureServiceBus(_EventBusMQ);
                        break;
                    case EventBusMQ.AmazonSQS:
                        x.UseAmazonSQS(opts =>
                        {
                            var uri = new Uri(_EventBusMQ);
                            if (!string.IsNullOrEmpty(uri.UserInfo) && uri.UserInfo?.Split(':').Length == 2)
                            {
                                var userinfo = uri.UserInfo.Split(':');
                                opts.Credentials = new Amazon.Runtime.BasicAWSCredentials(userinfo[0], userinfo[1]);
                            }
                            opts.Region = Amazon.RegionEndpoint.GetBySystemName(uri.Host);
                        });
                        break;
                    case EventBusMQ.RedisStreams:
                        x.UseRedis(_EventBusMQ);
                        break;
                    case EventBusMQ.NATS:
                        x.UseNATS(_EventBusMQ);
                        break;
                    case EventBusMQ.Pulsar:
                        x.UsePulsar(_EventBusMQ);
                        break;
                    case EventBusMQ.InMemory:
                    default:
                        x.UseInMemoryMessageQueue();
                        break;
                }
                x.UseDashboard();
                //x.UseDiscovery();
            });
        }

        
    }
}
