using CoAP.Server.Routing;
using IoTSharp.Services.Coap;
using System.Threading.Tasks;
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
        public void CoapNetRouteEndpointDoesNotRegisterShortPathEndpoints()
        {
            var devices = Assert.Single(CoapRouteEndpoint.Create(new[]
            {
                CoAP.Server.Routing.CoapRoute.Post("devices/{device}/telemetry", _ => new ValueTask<CoapRouteResult>(CoapRouteResult.Changed()))
            }));
            var target = devices.GetChild("device-001");

            Assert.NotNull(target);
            Assert.NotNull(target.GetChild("telemetry"));
            Assert.Null(target.GetChild("Telemetry"));
        }
    }
}
