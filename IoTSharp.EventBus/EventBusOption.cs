using DotNetCore.CAP.Dashboard.NodeDiscovery;
using MaiKeBing.HostedService.ZeroMQ;
using Microsoft.Extensions.DependencyInjection;
using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class EventBusOption
    {
        public AppSettings AppSettings { get; set; }
        public ZMQOption ZMQOption { get; set; }
        public DiscoveryOptions DiscoveryOptions { get; set; }
        public string EventBusStore { get; set; }
        public string EventBusMQ { get; set; }
        public IHealthChecksBuilder HealthChecks { get; set; }

        public delegate Task RunRulesEventHander(Guid devid, object obj, MountType mountType);

        public  RunRulesEventHander RunRules;
    }
}
