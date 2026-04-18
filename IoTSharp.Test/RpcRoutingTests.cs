#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MQTTnet.AspNetCore.Routing;
using Xunit;

namespace IoTSharp.Test
{
    public sealed class RpcRoutingTests
    {
        [Fact]
        public void Uplink_Rpc_Route_Matches_Exact_Request_Topic()
        {
            var result = MatchTopic("devices/me/rpc/request/reboot");

            Assert.Equal(nameof(TestRpcController.Request), result.HandlerName);
            Assert.Equal("me", result.Parameters["devname"]);
            Assert.Equal("reboot", result.Parameters["method"]);
        }

        [Fact]
        public void Uplink_Rpc_Route_Does_Not_Match_Downlink_Request_Topic()
        {
            var result = MatchTopic("devices/me/rpc/request/reboot/req-001");

            Assert.Null(result.HandlerName);
            Assert.Empty(result.Parameters);
        }

        private static RouteMatchResult MatchTopic(string topic)
        {
            var assembly = typeof(MqttRouteAttribute).Assembly;
            var tableFactory = assembly.GetType("MQTTnet.AspNetCore.Routing.MqttRouteTableFactory")!;
            var create = tableFactory.GetMethod(
                "Create",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(IEnumerable<Assembly>)],
                modifiers: null)!;
            var routeTable = create.Invoke(null, [new[] { typeof(TestRpcController).Assembly }])!;
            var routeMethod = routeTable.GetType().GetMethod("Route", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            var contextType = assembly.GetType("MQTTnet.AspNetCore.Routing.MqttRouteContext")!;
            var handlerProperty = contextType.GetProperty("Handler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
            var parametersProperty = contextType.GetProperty("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
            var context = Activator.CreateInstance(contextType, topic)!;

            routeMethod.Invoke(routeTable, [context]);

            var handler = handlerProperty.GetValue(context) as MethodInfo;
            var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
            if (parametersProperty.GetValue(context) is IEnumerable entries)
            {
                foreach (var entry in entries)
                {
                    var entryType = entry.GetType();
                    var key = entryType.GetProperty("Key")!.GetValue(entry)?.ToString();
                    var value = entryType.GetProperty("Value")!.GetValue(entry)?.ToString();
                    if (!string.IsNullOrEmpty(key) && value is not null)
                    {
                        parameters[key] = value;
                    }
                }
            }

            return new RouteMatchResult(handler?.Name, parameters);
        }

        private sealed record RouteMatchResult(string? HandlerName, IReadOnlyDictionary<string, string> Parameters);

        [MqttController]
        [MqttRoute("devices/{devname}/rpc")]
        private sealed class TestRpcController
        {
            [MqttRoute("request/{method}")]
            public void Request(string method)
            {
            }
        }
    }
}
