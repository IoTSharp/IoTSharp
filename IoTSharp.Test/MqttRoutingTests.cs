#nullable enable

using System;
using IoTSharp.Contracts;
using IoTSharp.Services.MQTTControllers;
using MQTTnet.AspNetCore.Routing.Testing;
using Xunit;

namespace IoTSharp.Test
{
    public sealed class MqttRoutingTests : IDisposable
    {
        private readonly MqttRoutingTestHost _host = MqttRoutingTestHost.Create(services =>
            services.AddIoTSharpMqttServer(new MqttBrokerSetting()));

        [Theory]
        [InlineData("devices/me/telemetry", typeof(TelemetryController), nameof(TelemetryController.telemetry))]
        [InlineData("devices/me/Telemetry", typeof(TelemetryController), nameof(TelemetryController.telemetry))]
        [InlineData("devices/me/attributes", typeof(AttributesController), nameof(AttributesController.attributes))]
        [InlineData("devices/me/Attributes", typeof(AttributesController), nameof(AttributesController.attributes))]
        [InlineData("devices/me/alarm", typeof(AlarmController), nameof(AlarmController.UpdateStatus))]
        [InlineData("devices/me/Alarm", typeof(AlarmController), nameof(AlarmController.UpdateStatus))]
        [InlineData("devices/me/data/binary", typeof(DataController), nameof(DataController.BinaryDataProcessing))]
        [InlineData("devices/me/Data/binary", typeof(DataController), nameof(DataController.BinaryDataProcessing))]
        [InlineData("devices/me/data/json", typeof(DataController), nameof(DataController.JsonDataProcessing))]
        [InlineData("devices/me/Data/json", typeof(DataController), nameof(DataController.JsonDataProcessing))]
        [InlineData("devices/me/rpc/request/RebootNow", typeof(RpcController), nameof(RpcController.request))]
        [InlineData("devices/me/Rpc/request/RebootNow", typeof(RpcController), nameof(RpcController.request))]
        [InlineData("gateway/telemetry", typeof(GatewayController), nameof(GatewayController.telemetry))]
        [InlineData("Gateway/telemetry", typeof(GatewayController), nameof(GatewayController.telemetry))]
        [InlineData("v1/gateway/telemetry", typeof(V1GatewayController), nameof(V1GatewayController.telemetry))]
        public void Topic_Matches_Production_Controller(string topic, Type controllerType, string actionName)
        {
            var match = _host.Match(topic);

            Assert.True(match.IsMatched);
            Assert.Equal(controllerType, match.Route!.ControllerType);
            Assert.Equal(actionName, match.Route.ActionMethod!.Name);
        }

        [Theory]
        [InlineData("devices/Meter-A/telemetry/xml/PressurePSI", "keyname", "PressurePSI")]
        [InlineData("devices/Meter-A/Telemetry/binary/PressurePSI", "keyname", "PressurePSI")]
        [InlineData("devices/Meter-A/attributes/request/Request-A", "requestid", "Request-A")]
        [InlineData("devices/Meter-A/Attributes/request/Request-A", "requestid", "Request-A")]
        [InlineData("devices/Meter-A/rpc/request/RebootNow", "method", "RebootNow")]
        [InlineData("devices/Meter-A/Rpc/request/RebootNow", "method", "RebootNow")]
        public void Route_Values_Preserve_Case(string topic, string parameter, string value)
        {
            var match = _host.Match(topic);

            Assert.Equal("Meter-A", match.GetRouteValue<string>("devname"));
            Assert.Equal(value, match.GetRouteValue<string>(parameter));
        }

        [Theory]
        [InlineData("Devices/me/telemetry")]
        [InlineData("devices/me/TELEMETRY")]
        [InlineData("devices/me/ATTRIBUTES")]
        [InlineData("devices/me/data/Binary")]
        [InlineData("devices/me/RPC/request/RebootNow")]
        [InlineData("devices/me/rpc/request/RebootNow/req-001")]
        [InlineData("devices/me/Rpc/request/RebootNow/req-001")]
        [InlineData("v1/Gateway/telemetry")]
        public void Unregistered_Topic_Does_Not_Match(string topic)
        {
            Assert.False(_host.Match(topic).IsMatched);
        }

        public void Dispose() => _host.Dispose();
    }
}
