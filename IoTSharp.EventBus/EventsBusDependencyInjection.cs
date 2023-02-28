

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using static IoTSharp.EventBus.EventBusOption;

namespace IoTSharp.EventBus
{
    public static class EventsBusDependencyInjection
    {

        public static IApplicationBuilder UseEventBus(this IApplicationBuilder app, Func<EventBusOption, RunRulesEventHander>  action)
        {
            var provider = app.ApplicationServices;
            var options = provider.GetService<EventBusOption>();
            var hander=   action.Invoke(options!);
            if (options != null)
            {
                options.RunRules += hander;
            }
            return app;

        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOption> opt)
        {
            var options = new EventBusOption();
            options.services = services;
            opt.Invoke(options);
            var settings = options.AppSettings;
            var healthChecks = options.HealthChecks;
            var _EventBusStore = options.EventBusStore;
            var _EventBusMQ = options.EventBusMQ;
            services.AddSingleton(options);
          
            return services;
        }

   
    }
}
