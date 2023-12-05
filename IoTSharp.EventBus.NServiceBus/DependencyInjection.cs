
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Contracts;

namespace IoTSharp.EventBus.NServiceBus
{
    public static class DependencyInjection
    {

        public static IApplicationBuilder UseNServiceBusEventBus(this IApplicationBuilder app)
        {
            var provider = app.ApplicationServices;
            var options = provider.GetService<EventBusOption>();
            return app;
        }

        public static void UseNServiceBus(this EventBusOption  opt)
        {
            var settings = opt.AppSettings;
            var healthChecks = opt.HealthChecks;
            var _EventBusStore = opt.EventBusStore;
            var _EventBusMQ = opt.EventBusMQ;
            var services = opt.services;
            services.AddTransient<ISubscriber, NSBusSubscriber>();
            services.AddTransient<IPublisher, NSBusPublisher>();
        }

        
    }
}
