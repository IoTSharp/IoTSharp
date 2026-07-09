using BenchmarkDotNet.Attributes;
using CoAP;
using CoAP.Server.Resources;
using CoAP.Server.Routing;
using IoTSharp.Services.Coap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTSharp.Benchmarks.Benchmarks
{
    /// <summary>
    /// CoAP route 匹配基准，覆盖 IoTSharp 平台约定与 CoAP.NET endpoint matcher。
    /// </summary>
    [MemoryDiagnoser]
    [BenchmarkCategory("Coap")]
    public class CoapRouteMatchingBenchmark
    {
        private CoapEndpointDataSource _dataSource;
        private CoapEndpointMatcher _matcher;
        private CoapEndpointMatchContext _platformMatchContext;
        private string[] _platformSegments;

        [GlobalSetup]
        public void Setup()
        {
            var endpoints = new List<CoapEndpoint>(CoapPlatformRouteConventions.RecommendedRoutes.Count + 128);
            foreach (var route in CoapPlatformRouteConventions.RecommendedRoutes)
            {
                endpoints.Add(new CoapEndpoint(
                    Method.POST,
                    route.Template,
                    HandleAsync,
                    new object[]
                    {
                        new CoapConsumesAttribute(MediaType.ApplicationJson),
                        new CoapProducesAttribute(MediaType.TextPlain),
                    },
                    "IoTSharp " + route.Template));
            }

            for (var i = 0; i < 128; i++)
            {
                endpoints.Add(new CoapEndpoint(
                    Method.POST,
                    "products/p" + i + "/devices/{device}/telemetry",
                    HandleAsync,
                    new object[]
                    {
                        new CoapConsumesAttribute(MediaType.ApplicationJson),
                        new CoapProducesAttribute(MediaType.TextPlain),
                    },
                    "Generated product telemetry " + i));
            }

            _dataSource = new CoapEndpointDataSource(endpoints);
            _matcher = new CoapEndpointMatcher(_dataSource);
            _platformSegments = new[] { "devices", "device-001", "telemetry" };
            _platformMatchContext = new CoapEndpointMatchContext(
                Method.POST,
                _platformSegments,
                MediaType.ApplicationJson,
                MediaType.TextPlain,
                observe: null);
        }

        [Benchmark(Baseline = true, Description = "IoTSharp platform convention")]
        public CoapPlatformOperation MatchPlatformConvention()
        {
            if (!CoapPlatformRouteConventions.TryMatch(_platformSegments, out var match))
            {
                throw new InvalidOperationException("IoTSharp platform route did not match.");
            }

            return match.Operation;
        }

        [Benchmark(Description = "CoAP.NET endpoint matcher")]
        public string MatchCoapNetEndpoint()
        {
            if (!_matcher.TryMatch(_platformMatchContext, out var match))
            {
                throw new InvalidOperationException("CoAP.NET endpoint did not match.");
            }

            return match.RouteValues[CoapPlatformRouteConventions.DeviceRouteValueName];
        }

        [Benchmark(Description = "CoAP.NET resource tree build")]
        public IReadOnlyList<IResource> BuildResourceTree()
        {
            return CoapRouteEndpoint.Create(_dataSource);
        }

        private static ValueTask<CoapRouteResult> HandleAsync(CoapRouteContext context)
        {
            return new ValueTask<CoapRouteResult>(CoapRouteResult.Changed());
        }
    }
}
