
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using static IoTSharp.EventBus.EventBusOption;
using Shashlik.EventBus;
using Shashlik.EventBus.PostgreSQL;
using Shashlik.EventBus.MySql;
using Shashlik.EventBus.SqlServer;
using Shashlik.EventBus.MemoryStorage;
using Shashlik.EventBus.RabbitMQ;
using Shashlik.EventBus.Kafka;
using Shashlik.EventBus.MemoryQueue;

namespace IoTSharp.EventBus.Shashlik
{
    public static class DependencyInjection
    {

        public static IApplicationBuilder UseShashlikEventBus(this IApplicationBuilder app)
        {
            var provider = app.ApplicationServices;
            var options = provider.GetService<EventBusOption>();
            return app;
        }

        public static void UserShashlik(this EventBusOption opt)
        {
            var settings = opt.AppSettings;
            var healthChecks = opt.HealthChecks;
            var _EventBusStore = opt.EventBusStore;
            var _EventBusMQ = opt.EventBusMQ;
            var services = opt.services;

            services.AddTransient<ISubscriber, ShashlikSubscriber>();
            services.AddTransient<IPublisher, ShashlikPublisher>();
            string _hc_EventBusStore = $"{nameof(EventBusStore)}-{Enum.GetName(settings.EventBusStore)}";
            var builder = services.AddEventBus((EventBusOptions opts) =>
            {
                opts.SucceedExpireHour = settings.SucceedMessageExpiredAfter;
                //   x.ConsumerThreadCount = settings.ConsumerThreadCount;
            });

            switch (settings.EventBusStore)
            {
                case EventBusStore.PostgreSql:
                    builder.AddNpgsql(_EventBusStore);
                    healthChecks.AddNpgSql(_EventBusStore, name: _hc_EventBusStore);
                    break;

                case EventBusStore.MongoDB:

                    break;
                case EventBusStore.LiteDB:
                    break;
                case EventBusStore.MySql:
                    builder.AddMySql(_EventBusStore);
                    break;
                case EventBusStore.SqlServer:
                    builder.AddSqlServer(_EventBusStore);
                    break;
                case EventBusStore.InMemory:
                default:
                    builder.AddMemoryStorage();
                    break;
            }
            string _hc_EventBusMQ = $"{nameof(EventBusMQ)}-{Enum.GetName(settings.EventBusMQ)}";
            switch (settings.EventBusMQ)
            {
                case EventBusMQ.RabbitMQ:
                    var url = new Uri(_EventBusMQ);
                    builder.AddRabbitMQ(cfg =>
                    {
                        cfg.Host = url.Host;
                        var userinfo = url.UserInfo?.Split(':');
                        if (userinfo != null)
                        {
                            cfg.UserName = userinfo[0];
                            cfg.Password = userinfo[1];
                        }
                    });
                    break;

                case EventBusMQ.Kafka:
                    builder.AddKafka(cfg =>
                    {
                        cfg.AddOrUpdate("bootstrap.servers", _EventBusMQ);
                        cfg.AddOrUpdate("allow.auto.create.topics", "true");
                    });
                    break;
                case EventBusMQ.InMemory:
                    builder.AddMemoryQueue();
                    break;
                case EventBusMQ.ZeroMQ:
                    break;
                case EventBusMQ.AzureServiceBus:
                    break;
                case EventBusMQ.AmazonSQS:
                    break;
                case EventBusMQ.RedisStreams:
                    break;
                case EventBusMQ.NATS:
                    break;
                case EventBusMQ.Pulsar:
                    break;

                default:
                    break;
            }
        }
    }
}
