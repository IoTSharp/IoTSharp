using Microsoft.Extensions.DependencyInjection;
using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class EventBusOption
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public AppSettings AppSettings { get; set; }

        public EventBusFramework EventBus => AppSettings.EventBus;



        public string EventBusStore { get; set; }

        public string EventBusMQ { get; set; }
        public IHealthChecksBuilder HealthChecks { get; set; } 
        public IServiceCollection services { get; internal set; }

        public delegate Task RunRulesEventHander(Guid devid, object obj, EventType mountType);

        public  RunRulesEventHander RunRules;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    }
}
