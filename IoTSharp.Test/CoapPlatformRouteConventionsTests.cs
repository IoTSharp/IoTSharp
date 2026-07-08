using CoAP.Server.Routing;
using IoTSharp.Services.Coap;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace IoTSharp.Test
{
    public class CoapPlatformRouteConventionsTests
    {
        [Fact]
        public void RecommendedRoutesUsePlatformRouteNames()
        {
            Assert.Contains(CoapPlatformRouteConventions.RecommendedRoutes, route => route.Template == "devices/{device}/telemetry");
            Assert.Contains(CoapPlatformRouteConventions.RecommendedRoutes, route => route.Template == "devices/{device}/attributes");
            Assert.Contains(CoapPlatformRouteConventions.RecommendedRoutes, route => route.Template == "devices/{device}/alarm");
            Assert.Contains(CoapPlatformRouteConventions.RecommendedRoutes, route => route.Template == "gateways/{gateway}/telemetry");
        }

        [Fact]
        public void TryMatchAcceptsDeviceTelemetryRoute()
        {
            var matched = CoapPlatformRouteConventions.TryMatch(new[] { "devices", "device-001", "telemetry" }, out var route);

            Assert.True(matched);
            Assert.Equal(CoapPlatformTargetKind.Device, route.TargetKind);
            Assert.Equal("device-001", route.TargetName);
            Assert.Equal(CoapPlatformOperation.Telemetry, route.Operation);
        }

        [Fact]
        public void TryMatchRejectsGatewayAlarmRoute()
        {
            var matched = CoapPlatformRouteConventions.TryMatch(new[] { "gateways", "gw-001", "alarm" }, out _);

            Assert.False(matched);
        }

        [Fact]
        public void TryMatchRejectsShortPathsOutsideRouteContract()
        {
            var matched = CoapPlatformRouteConventions.TryMatch(new[] { "Telemetry" }, out _);

            Assert.False(matched);
        }

        [Fact]
        public void CoapResourceDiscoveryRegistersRecommendedPlatformRoutes()
        {
            var services = new ServiceCollection();
            services.AddCoapResources(options => options.AddApplicationPart<DeviceCoapResource>());

            using var provider = services.BuildServiceProvider();
            var endpoints = provider.GetRequiredService<ICoapEndpointDataSource>().Endpoints;
            var templates = endpoints
                .Where(endpoint => endpoint.Method == CoAP.Method.POST)
                .Select(endpoint => endpoint.RoutePattern.Template)
                .ToArray();

            Assert.Contains("devices/{device}/telemetry", templates);
            Assert.Contains("devices/{device}/attributes", templates);
            Assert.Contains("devices/{device}/alarm", templates);
            Assert.Contains("gateways/{gateway}/telemetry", templates);
            Assert.Contains("gateways/{gateway}/attributes", templates);
            Assert.DoesNotContain("Telemetry", templates);
        }
    }
}
