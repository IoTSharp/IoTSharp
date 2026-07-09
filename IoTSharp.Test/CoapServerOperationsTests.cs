using CoAP;
using CoAP.Server;
using CoAP.Server.Hosting;
using CoAP.Server.Routing;
using IoTSharp.Services.Coap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    [Collection("CoapProtocol")]
    public class CoapServerOperationsTests
    {
        [Fact]
        public void DisabledCoapDoesNotRegisterHostedServer()
        {
            using var provider = BuildProvider(
                ("CoapServer:Enabled", "false"),
                ("CoapServer:Dtls:Enabled", "false"));

            Assert.Empty(provider.GetServices<IHostedService>());
            var options = provider.GetRequiredService<CoapServerListenOptions>();
            Assert.False(options.HasEnabledTransport);
        }

        [Fact]
        public async Task EnabledCoapStartsWithConfiguredBindAddress()
        {
            CoapLogging.LoggerFactory = NullLoggerFactory.Instance;
            using var provider = BuildProvider(
                ("CoapServer:Enabled", "true"),
                ("CoapServer:BindAddress", "127.0.0.1"),
                ("CoapServer:Port", "0"),
                ("CoapServer:Dtls:Enabled", "false"));

            await StartHostedServicesAsync(provider);
            try
            {
                var endpoint = Assert.Single(provider.GetRequiredService<IServer>().EndPoints);
                var localEndpoint = Assert.IsType<IPEndPoint>(endpoint.LocalEndPoint);
                Assert.Equal(IPAddress.Loopback, localEndpoint.Address);
                Assert.True(localEndpoint.Port > 0);
                Assert.Same(provider.GetRequiredService<ILoggerFactory>(), CoapLogging.LoggerFactory);
            }
            finally
            {
                await StopHostedServicesAsync(provider);
            }
        }

        [Fact]
        public async Task EnabledCoapFailsWhenPortIsAlreadyBound()
        {
            var port = GetFreeUdpPort();
            using var first = BuildProvider(
                ("CoapServer:Enabled", "true"),
                ("CoapServer:BindAddress", "127.0.0.1"),
                ("CoapServer:Port", port.ToString()),
                ("CoapServer:Dtls:Enabled", "false"));
            using var second = BuildProvider(
                ("CoapServer:Enabled", "true"),
                ("CoapServer:BindAddress", "127.0.0.1"),
                ("CoapServer:Port", port.ToString()),
                ("CoapServer:Dtls:Enabled", "false"));

            await StartHostedServicesAsync(first);
            try
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => StartHostedServicesAsync(second));
                Assert.Contains("None of the server's endpoints could be started", exception.Message);
            }
            finally
            {
                await StopHostedServicesAsync(second);
                await StopHostedServicesAsync(first);
            }
        }

        [Fact]
        public void CoapsRequiresPskKeysWhenEnabled()
        {
            var configuration = BuildConfiguration(
                ("CoapServer:Enabled", "false"),
                ("CoapServer:Dtls:Enabled", "true"),
                ("CoapServer:Dtls:BindAddress", "127.0.0.1"),
                ("CoapServer:Dtls:Port", "0"));
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Assert.Throws<InvalidOperationException>(
                () => services.AddCoapServer(configuration.GetSection("CoapServer")));
            Assert.Contains("CoapServer:Dtls:PskKeys", exception.Message);
        }

        [Fact]
        public void ConfigurationBinderAppliesProtocolAndDtlsOptions()
        {
            using var provider = BuildProvider(
                ("CoapServer:Enabled", "false"),
                ("CoapServer:DefaultPort", "15683"),
                ("CoapServer:DefaultSecurePort", "15684"),
                ("CoapServer:AckRandomFactor", "1.25"),
                ("CoapServer:MaxRetransmit", "7"),
                ("CoapServer:ChannelReceivePacketSize", "4096"),
                ("CoapServer:Dtls:Enabled", "true"),
                ("CoapServer:Dtls:BindAddress", "localhost"),
                ("CoapServer:Dtls:SessionIdleSeconds", "45"),
                ("CoapServer:Dtls:PskKeys:device-1", "shared-secret"));

            var options = provider.GetRequiredService<CoapServerListenOptions>();

            Assert.False(options.Enabled);
            Assert.Equal(15683, options.DefaultPort);
            Assert.Equal(15684, options.GetCoapsPort());
            Assert.Equal(1.25, options.AckRandomFactor);
            Assert.Equal(7, options.MaxRetransmit);
            Assert.Equal(4096, options.ChannelReceivePacketSize);
            Assert.True(options.Dtls.Enabled);
            Assert.Equal(IPAddress.Loopback, options.GetCoapsBindAddress());
            Assert.Equal(TimeSpan.FromSeconds(45), options.Dtls.GetSessionIdleTimeout());
            Assert.Equal("shared-secret", options.Dtls.PskKeys["device-1"]);
        }

        [Fact]
        public async Task CoapsStartsOnlyWhenDtlsPskIsConfigured()
        {
            using var provider = BuildProvider(
                ("CoapServer:Enabled", "false"),
                ("CoapServer:Dtls:Enabled", "true"),
                ("CoapServer:Dtls:BindAddress", "127.0.0.1"),
                ("CoapServer:Dtls:Port", "0"),
                ("CoapServer:Dtls:PskKeys:test-device", "test-secret"));

            await StartHostedServicesAsync(provider);
            try
            {
                var endpoint = Assert.Single(provider.GetRequiredService<IServer>().EndPoints);
                var localEndpoint = Assert.IsType<IPEndPoint>(endpoint.LocalEndPoint);
                Assert.Equal(IPAddress.Loopback, localEndpoint.Address);
                Assert.True(localEndpoint.Port > 0);
            }
            finally
            {
                await StopHostedServicesAsync(provider);
            }
        }

        [Fact]
        public async Task UnauthorizedBusinessResultMapsToCoap401()
        {
            var result = await CoapPlatformResourceDispatcher.DispatchAsync(
                new FixedDispatcher(CoapBusinessDispatchResult.Fail(
                    CoapBusinessDispatchStatus.Unauthorized,
                    "Access token is missing or invalid.")),
                new CoapPlatformRouteMatch(
                    CoapPlatformTargetKind.Device,
                    "device-001",
                    CoapPlatformOperation.Telemetry),
                CreateProtocolContext());

            Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
            Assert.Equal("Access token is missing or invalid.", Encoding.UTF8.GetString(result.Payload.Span));
        }

        [Fact]
        public async Task ErrorBusinessResultMapsToCoap500()
        {
            var result = await CoapPlatformResourceDispatcher.DispatchAsync(
                new FixedDispatcher(CoapBusinessDispatchResult.Fail(
                    CoapBusinessDispatchStatus.Error,
                    "CoAP business dispatch failed.")),
                new CoapPlatformRouteMatch(
                    CoapPlatformTargetKind.Device,
                    "device-001",
                    CoapPlatformOperation.Telemetry),
                CreateProtocolContext());

            Assert.Equal(StatusCode.InternalServerError, result.StatusCode);
            Assert.Equal("CoAP business dispatch failed.", Encoding.UTF8.GetString(result.Payload.Span));
        }

        private static ServiceProvider BuildProvider(params (string Key, string Value)[] settings)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var configuration = BuildConfiguration(settings);
            services.AddCoapServer(configuration.GetSection("CoapServer"));
            services.AddIoTSharpCoapResources();
            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfiguration(params (string Key, string Value)[] settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings.ToDictionary(c => c.Key, c => c.Value))
                .Build();
        }

        private static async Task StartHostedServicesAsync(IServiceProvider provider)
        {
            foreach (var hostedService in provider.GetServices<IHostedService>())
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        private static async Task StopHostedServicesAsync(IServiceProvider provider)
        {
            foreach (var hostedService in provider.GetServices<IHostedService>().Reverse())
            {
                await hostedService.StopAsync(CancellationToken.None);
            }

            CoapLogging.LoggerFactory = NullLoggerFactory.Instance;
        }

        private static int GetFreeUdpPort()
        {
            using var udp = new UdpClient(0);
            return ((IPEndPoint)udp.Client.LocalEndPoint).Port;
        }

        private static CoapRouteContext CreateProtocolContext()
        {
            var route = CoapRoute.Post(
                "devices/{device}/telemetry",
                _ => new ValueTask<CoapRouteResult>(CoapRouteResult.Changed()));
            return new CoapRouteContext(
                route,
                Method.POST,
                new[] { "devices", "device-001", "telemetry" },
                new Dictionary<string, string> { ["device"] = "device-001" },
                new[] { "access_token=invalid" },
                Encoding.UTF8.GetBytes("{\"temperature\":23}"),
                MediaType.ApplicationJson,
                MediaType.Undefined);
        }

        private sealed class FixedDispatcher : ICoapBusinessDispatcher
        {
            private readonly CoapBusinessDispatchResult _result;

            public FixedDispatcher(CoapBusinessDispatchResult result)
            {
                _result = result;
            }

            public ValueTask<CoapBusinessDispatchResult> DispatchAsync(
                CoapBusinessDispatchContext context,
                CancellationToken cancellationToken = default)
            {
                return new ValueTask<CoapBusinessDispatchResult>(_result);
            }
        }
    }
}
