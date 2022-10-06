using Microsoft.Extensions.DependencyInjection;
using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class EventBusOption
    {
        public AppSettings AppSettings { get; set; }

        public EventBusFramework EventBus => AppSettings.EventBus;

 
        public string EventBusStore { get; set; }
        public string EventBusMQ { get; set; }
        public IHealthChecksBuilder HealthChecks { get; set; }
        public IServiceCollection services { get; internal set; }

        public delegate Task RunRulesEventHander(Guid devid, object obj, EventType mountType);

        public  RunRulesEventHander RunRules;
    }
}
