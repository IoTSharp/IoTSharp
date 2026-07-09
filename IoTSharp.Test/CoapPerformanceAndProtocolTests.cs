using CoAP;
using CoAP.Net;
using CoAP.Observe;
using CoAP.Server.Resources;
using CoAP.Server.Hosting;
using CoAP.Server.Routing;
using IoTSharp.Services.Coap;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    [CollectionDefinition("CoapProtocol", DisableParallelization = true)]
    public sealed class CoapProtocolCollectionDefinition
    {
    }

    [Collection("CoapProtocol")]
    public class CoapPerformanceAndProtocolTests
    {
        [Fact]
        public async Task PlatformRouteDispatchesLargePayloadWithBlock1Metadata()
        {
            var payload = CreateJsonPayload(32 * 1024);
            var dispatcher = new CapturingDispatcher();
            var context = CreateProtocolContext(
                payload,
                new Option[]
                {
                    new BlockOption(OptionType.Block1, 0, BlockOption.EncodeSZX(512), false)
                });

            var result = await CoapPlatformResourceDispatcher.DispatchAsync(
                dispatcher,
                new CoapPlatformRouteMatch(
                    CoapPlatformTargetKind.Device,
                    "device-001",
                    CoapPlatformOperation.Telemetry),
                context);

            Assert.Equal(StatusCode.Changed, result.StatusCode);
            Assert.Equal(CoapPlatformTargetKind.Device, dispatcher.Route.TargetKind);
            Assert.Equal(CoapPlatformOperation.Telemetry, dispatcher.Route.Operation);
            Assert.Equal("device-001", dispatcher.Route.TargetName);
            Assert.Equal(payload, dispatcher.Payload);
            Assert.Contains("access_token=token-001", dispatcher.Queries);
            Assert.NotNull(dispatcher.ProtocolContext.Block1);
            Assert.Equal(512, dispatcher.ProtocolContext.Block1.Size);
            Assert.Equal(0, dispatcher.ProtocolContext.Block1.NUM);
            Assert.False(dispatcher.ProtocolContext.Block1.M);
        }

        [Fact]
        public void PayloadParserReadsLargeUtf8ObjectForBenchmarkBaseline()
        {
            var payload = CoapPayloadParser.ParseObject(CreateJsonPayload(32 * 1024));

            Assert.Equal(2, payload.Count);
            Assert.True(payload.ContainsKey("temperature"));
            Assert.True(payload.TryGetValue("sample", out var sample));
            Assert.True(((string)sample).Length > 30 * 1024);
        }

        [Fact]
        public void RouteEndpointObserveEstablishesRelationInMemory()
        {
            CoapLogging.LoggerFactory = NullLoggerFactory.Instance;

            var calls = 0;
            int? capturedObserve = null;
            var endpointDescriptor = new CoapEndpoint(
                Method.GET,
                "diagnostics/{target}/status",
                context =>
                {
                    calls++;
                    capturedObserve = context.Observe;
                    return new ValueTask<CoapRouteResult>(
                        CoapRouteResult.Text(StatusCode.Content, "value-" + calls)
                            .WithObserve(calls)
                            .WithMaxAge(5));
                },
                new object[]
                {
                    new CoapObserveAttribute("status"),
                    new CoapProducesAttribute(MediaType.TextPlain),
                    new CoapResourceTitleAttribute("Diagnostic status")
                });
            var dataSource = new CoapEndpointDataSource(new[] { endpointDescriptor });
            var matcher = new CoapEndpointMatcher(dataSource);
            var registry = new CoapRouteObserveRegistry();
            var endpoint = CreateEndpoint(dataSource, matcher, registry, "edge-01", "status");
            var request = Request.NewGet();
            request.Source = new IPEndPoint(IPAddress.Loopback, 56830);
            request.Token = new byte[] { 0x01 };
            request.MarkObserve();
            var exchange = new CapturingExchange(request);
            var remote = new ObservingEndpoint(request.Source);
            var relation = new ObserveRelation(new CoapConfig(), remote, endpoint, exchange);
            remote.AddObserveRelation(relation);
            exchange.Relation = relation;

            endpoint.HandleRequest(exchange);

            Assert.True(relation.Established);
            Assert.Equal(1, calls);
            Assert.Equal(0, capturedObserve);
            Assert.Equal(StatusCode.Content, exchange.SentResponse.StatusCode);
            Assert.Equal("value-1", exchange.SentResponse.PayloadString);
            Assert.Equal(1, exchange.SentResponse.Observe);
            Assert.True(registry.HasObservers("diagnostics/edge-01/status"));

            Assert.Equal(1, registry.NotifyObservers("diagnostics/edge-01/status"));

            Assert.Equal(2, calls);
            Assert.Equal("value-2", exchange.SentResponse.PayloadString);
            Assert.Equal(2, exchange.SentResponse.Observe);
        }

        [Fact]
        public void CoapsPskOptionsPreservePressureTestSettings()
        {
            var options = new CoapServerListenOptions
            {
                Enabled = false,
                DefaultBlockSize = 512,
                MaxMessageSize = 512,
                Dtls = new CoapServerDtlsPskOptions
                {
                    Enabled = true,
                    BindAddress = "127.0.0.1",
                    Port = 0,
                    SessionIdleSeconds = 15,
                    PskKeys = new Dictionary<string, string>
                    {
                        ["device-1"] = "shared-secret"
                    }
                }
            };

            options.Validate();

            Assert.True(options.HasEnabledTransport);
            Assert.Equal(IPAddress.Loopback, options.GetCoapsBindAddress());
            Assert.Equal(0, options.GetCoapsPort());
            Assert.Equal(512, options.DefaultBlockSize);
            Assert.Equal(512, options.MaxMessageSize);
            Assert.Equal("shared-secret", options.Dtls.PskKeys["device-1"]);
        }

        private static CoapRouteContext CreateProtocolContext(
            byte[] payload,
            IReadOnlyList<Option> options)
        {
            var route = CoapRoute.Post(
                "devices/{device}/telemetry",
                _ => new ValueTask<CoapRouteResult>(CoapRouteResult.Changed()));
            return new CoapRouteContext(
                route,
                Method.POST,
                new[] { "devices", "device-001", "telemetry" },
                new Dictionary<string, string> { ["device"] = "device-001" },
                new[] { "access_token=token-001" },
                payload,
                MediaType.ApplicationJson,
                MediaType.Undefined,
                options: options);
        }

        private static IResource CreateEndpoint(
            ICoapEndpointDataSource dataSource,
            ICoapEndpointMatcher matcher,
            CoapRouteObserveRegistry registry,
            params string[] childSegments)
        {
            IResource endpoint = CoapRouteEndpoint.Create(dataSource, matcher, null, registry).Single();
            foreach (var segment in childSegments)
            {
                endpoint = endpoint.GetChild(segment);
                Assert.NotNull(endpoint);
            }

            return endpoint;
        }

        private static byte[] CreateJsonPayload(int size)
        {
            var builder = new StringBuilder(size + 64);
            builder.Append("{\"temperature\":23.5,\"sample\":\"");
            while (builder.Length < size - 2)
            {
                builder.Append("abcdef0123456789");
            }

            builder.Append("\"}");
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private sealed class CapturingDispatcher : ICoapBusinessDispatcher
        {
            public CoapPlatformRouteMatch Route { get; private set; }

            public CoapRouteContext ProtocolContext { get; private set; }

            public byte[] Payload { get; private set; }

            public IReadOnlyList<string> Queries { get; private set; }

            public ValueTask<CoapBusinessDispatchResult> DispatchAsync(
                CoapBusinessDispatchContext context,
                CancellationToken cancellationToken = default)
            {
                Route = context.PlatformRoute;
                ProtocolContext = context.ProtocolContext;
                Payload = context.Payload.ToArray();
                Queries = context.Queries.ToArray();
                return new ValueTask<CoapBusinessDispatchResult>(CoapBusinessDispatchResult.Success());
            }
        }

        private sealed class CapturingExchange : Exchange
        {
            public CapturingExchange(Request request)
                : base(request, Origin.Remote)
            {
                Request = request ?? throw new ArgumentNullException(nameof(request));
            }

            public Response SentResponse { get; private set; }

            public override void SendResponse(Response response)
            {
                SentResponse = response;
                Response = response;
            }
        }
    }
}
