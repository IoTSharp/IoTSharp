using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using IoTSharp.Services.Coap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    public class CoapBusinessDispatcherTests
    {
        [Fact]
        public async Task DispatchTelemetryPublishesDevicePayload()
        {
            using var services = BuildServices();
            var deviceId = await SeedDeviceAsync(services, "device-001", DeviceType.Device, "token-001");
            var dispatcher = services.GetRequiredService<ICoapBusinessDispatcher>();
            var publisher = services.GetRequiredService<RecordingPublisher>();

            var result = await dispatcher.DispatchAsync(CreateContext(
                CoapPlatformTargetKind.Device,
                "device-001",
                CoapPlatformOperation.Telemetry,
                "access_token=token-001",
                "{\"temperature\":23}"));

            Assert.Equal(CoapBusinessDispatchStatus.Success, result.Status);
            Assert.Contains(deviceId, publisher.ActiveDevices);
            var telemetry = Assert.Single(publisher.TelemetryMessages);
            Assert.Equal(deviceId, telemetry.DeviceId);
            Assert.Equal(DataCatalog.TelemetryData, telemetry.DataCatalog);
            Assert.Equal(23, telemetry.MsgBody["temperature"]);
        }

        [Fact]
        public async Task DispatchAttributesPublishesGatewayPayload()
        {
            using var services = BuildServices();
            var gatewayId = await SeedDeviceAsync(services, "gateway-001", DeviceType.Gateway, "gateway-token");
            var dispatcher = services.GetRequiredService<ICoapBusinessDispatcher>();
            var publisher = services.GetRequiredService<RecordingPublisher>();

            var result = await dispatcher.DispatchAsync(CreateContext(
                CoapPlatformTargetKind.Gateway,
                "gateway-001",
                CoapPlatformOperation.Attributes,
                "token=gateway-token",
                "{\"firmware\":\"1.0.0\"}"));

            Assert.Equal(CoapBusinessDispatchStatus.Success, result.Status);
            var attributes = Assert.Single(publisher.AttributeMessages);
            Assert.Equal(gatewayId, attributes.DeviceId);
            Assert.Equal(DataCatalog.AttributeData, attributes.DataCatalog);
            Assert.Equal("1.0.0", attributes.MsgBody["firmware"]);
        }

        [Fact]
        public async Task DispatchRejectsMissingAccessToken()
        {
            using var services = BuildServices();
            await SeedDeviceAsync(services, "device-001", DeviceType.Device, "token-001");
            var dispatcher = services.GetRequiredService<ICoapBusinessDispatcher>();
            var publisher = services.GetRequiredService<RecordingPublisher>();

            var result = await dispatcher.DispatchAsync(CreateContext(
                CoapPlatformTargetKind.Device,
                "device-001",
                CoapPlatformOperation.Telemetry,
                "request_id=abc",
                "{\"temperature\":23}"));

            Assert.Equal(CoapBusinessDispatchStatus.Unauthorized, result.Status);
            Assert.Empty(publisher.TelemetryMessages);
            Assert.Empty(publisher.ActiveDevices);
        }

        [Fact]
        public async Task DispatchRejectsRouteTargetMismatch()
        {
            using var services = BuildServices();
            await SeedDeviceAsync(services, "device-001", DeviceType.Device, "token-001");
            var dispatcher = services.GetRequiredService<ICoapBusinessDispatcher>();
            var publisher = services.GetRequiredService<RecordingPublisher>();

            var result = await dispatcher.DispatchAsync(CreateContext(
                CoapPlatformTargetKind.Device,
                "device-002",
                CoapPlatformOperation.Telemetry,
                "access_token=token-001",
                "{\"temperature\":23}"));

            Assert.Equal(CoapBusinessDispatchStatus.Forbidden, result.Status);
            Assert.Empty(publisher.TelemetryMessages);
            Assert.Empty(publisher.ActiveDevices);
        }

        [Fact]
        public async Task DispatchAlarmPublishesDeviceAlarm()
        {
            using var services = BuildServices();
            await SeedDeviceAsync(services, "device-001", DeviceType.Device, "token-001");
            var dispatcher = services.GetRequiredService<ICoapBusinessDispatcher>();
            var publisher = services.GetRequiredService<RecordingPublisher>();

            var result = await dispatcher.DispatchAsync(CreateContext(
                CoapPlatformTargetKind.Device,
                "device-001",
                CoapPlatformOperation.Alarm,
                "access_token=token-001",
                "{\"AlarmType\":\"HighTemperature\",\"AlarmDetail\":\"Temperature is high\"}"));

            Assert.Equal(CoapBusinessDispatchStatus.Success, result.Status);
            var alarm = Assert.Single(publisher.Alarms);
            Assert.Equal("device-001", alarm.OriginatorName);
            Assert.Equal(OriginatorType.Device, alarm.OriginatorType);
            Assert.Equal("HighTemperature", alarm.AlarmType);
        }

        private static ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();
            var publisher = new RecordingPublisher();
            var databaseName = Guid.NewGuid().ToString("N");

            services.AddLogging(builder => builder.AddDebug());
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddSingleton<IDataBaseModelBuilderOptions, TestModelBuilderOptions>();
            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.UseInternalServiceProvider(provider);
            });
            services.AddSingleton(publisher);
            services.AddSingleton<IPublisher>(publisher);
            services.AddSingleton<ICoapBusinessDispatcher, CoapBusinessDispatcher>();

            return services.BuildServiceProvider();
        }

        private static async Task<Guid> SeedDeviceAsync(IServiceProvider services, string name, DeviceType deviceType, string accessToken)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var device = deviceType == DeviceType.Gateway
                ? new Gateway { Id = Guid.NewGuid(), Name = name, DeviceType = deviceType, Timeout = 300 }
                : new Device { Id = Guid.NewGuid(), Name = name, DeviceType = deviceType, Timeout = 300 };
            var identity = new DeviceIdentity
            {
                Id = Guid.NewGuid(),
                Device = device,
                DeviceId = device.Id,
                IdentityType = IdentityType.AccessToken,
                IdentityId = accessToken
            };

            device.DeviceIdentity = identity;
            dbContext.Device.Add(device);
            dbContext.DeviceIdentities.Add(identity);
            await dbContext.SaveChangesAsync();
            return device.Id;
        }

        private static CoapBusinessDispatchContext CreateContext(
            CoapPlatformTargetKind targetKind,
            string targetName,
            CoapPlatformOperation operation,
            string query,
            string payload)
        {
            var root = targetKind == CoapPlatformTargetKind.Device
                ? CoapPlatformRouteConventions.DevicesRoot
                : CoapPlatformRouteConventions.GatewaysRoot;
            var operationSegment = operation switch
            {
                CoapPlatformOperation.Telemetry => CoapPlatformRouteConventions.TelemetrySegment,
                CoapPlatformOperation.Attributes => CoapPlatformRouteConventions.AttributesSegment,
                _ => CoapPlatformRouteConventions.AlarmSegment
            };
            var template = $"{root}/{{{CoapPlatformRouteConventions.GetTargetRouteValueName(targetKind)}}}/{operationSegment}";
            var protocolRoute = CoAP.Server.Routing.CoapRoute.Post(
                template,
                _ => new ValueTask<CoAP.Server.Routing.CoapRouteResult>(CoAP.Server.Routing.CoapRouteResult.Changed()));
            var routeValues = new Dictionary<string, string>
            {
                [CoapPlatformRouteConventions.GetTargetRouteValueName(targetKind)] = targetName
            };

            var protocolContext = new CoAP.Server.Routing.CoapRouteContext(
                protocolRoute,
                CoAP.Method.POST,
                new[] { root, targetName, operationSegment },
                routeValues,
                new[] { query },
                Encoding.UTF8.GetBytes(payload),
                CoAP.MediaType.ApplicationJson,
                CoAP.MediaType.Undefined);
            return new CoapBusinessDispatchContext(new CoapPlatformRouteMatch(targetKind, targetName, operation), protocolContext);
        }

        private sealed class TestModelBuilderOptions : IDataBaseModelBuilderOptions
        {
            public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

            public void OnModelCreating(ModelBuilder modelBuilder)
            {
            }
        }

        private sealed class RecordingPublisher : IPublisher
        {
            public List<Guid> ActiveDevices { get; } = new();

            public List<PlayloadData> AttributeMessages { get; } = new();

            public List<PlayloadData> TelemetryMessages { get; } = new();

            public List<CreateAlarmDto> Alarms { get; } = new();

            public Task<EventBusMetrics> GetMetrics() => Task.FromResult(new EventBusMetrics());

            public Task PublishCreateDevice(Guid devid) => Task.CompletedTask;

            public Task PublishDeleteDevice(Guid devid) => Task.CompletedTask;

            public Task PublishAttributeData(PlayloadData msg)
            {
                AttributeMessages.Add(msg);
                return Task.CompletedTask;
            }

            public Task PublishTelemetryData(PlayloadData msg)
            {
                TelemetryMessages.Add(msg);
                return Task.CompletedTask;
            }

            public Task PublishConnect(Guid devid, ConnectStatus devicestatus) => Task.CompletedTask;

            public Task PublishActive(Guid devid, ActivityStatus activity)
            {
                ActiveDevices.Add(devid);
                return Task.CompletedTask;
            }

            public Task PublishDeviceAlarm(CreateAlarmDto alarmDto)
            {
                Alarms.Add(alarmDto);
                return Task.CompletedTask;
            }
        }
    }
}
