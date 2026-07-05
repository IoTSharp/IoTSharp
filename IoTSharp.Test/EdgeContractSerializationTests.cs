#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using IoTSharp.Contracts;
using Xunit;

namespace IoTSharp.Test;

public sealed class EdgeContractSerializationTests
{
    [Fact]
    public void EdgeRuntimeDtos_ArePublishedFromContractsAssembly()
    {
        Assert.Equal("IoTSharp.Contracts", typeof(EdgeRegistrationDto).Namespace);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeTaskRequestDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeTaskDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeRuntimeStatusDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeCapabilityDto).Assembly);
        Assert.Equal(EdgeNodeContractVersions.EdgeRuntimeV1, new EdgeRegistrationDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeNodeV1, new EdgeNodeDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeRuntimeStatusV1, new EdgeRuntimeStatusDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeCapabilityV1, new EdgeCapabilityDto().ContractVersion);
    }

    [Fact]
    public void EdgeRuntimeStatusDto_SerializesStructuredMetrics()
    {
        var dto = new EdgeRuntimeStatusDto
        {
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            RuntimeType = EdgeRuntimeTypes.Gateway,
            RuntimeName = "gateway-a",
            Version = "1.0.0",
            InstanceId = "gateway-a-01",
            HostName = "host-a",
            Status = EdgeNodeStatusNames.Running,
            Healthy = true,
            UptimeSeconds = 123,
            Metadata = new Dictionary<string, object> { ["site"] = "plant-a" },
            Metrics = new Dictionary<string, object> { ["cpu"] = 0.25, ["memoryMb"] = 128 }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeRuntimeStatusDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-runtime-status-v1\"", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(EdgeNodeStatusNames.Running, roundtrip!.Status);
        Assert.True(roundtrip.Healthy);
        Assert.True(roundtrip.Metadata.ContainsKey("site"));
        Assert.True(roundtrip.Metrics.ContainsKey("cpu"));
    }

    [Fact]
    public void EdgeCapabilityDto_SerializesStructuredTasksAndCompatibility()
    {
        var dto = new EdgeCapabilityDto
        {
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            RuntimeType = EdgeRuntimeTypes.Gateway,
            RuntimeName = "gateway-a",
            Version = "1.0.0",
            InstanceId = "gateway-a-01",
            Protocols = ["modbus-tcp"],
            SupportedProtocols = [CollectionProtocolType.Modbus],
            SupportedPointTypes = ["holding-register"],
            SupportedTransforms = [CollectionTransformType.Scale, CollectionTransformType.Offset],
            SupportedReportTriggers = [ReportTriggerType.OnChange],
            Features = ["collection"],
            Tasks = [nameof(EdgeTaskType.HealthProbe)],
            TaskCapabilities =
            [
                new EdgeTaskCapabilityDto
                {
                    TaskType = nameof(EdgeTaskType.HealthProbe),
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                    SupportsProgress = false
                }
            ],
            CompatibleContracts =
            [
                new EdgeContractCompatibilityDto
                {
                    ContractName = "edge-task",
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1
                }
            ],
            Metadata = new Dictionary<string, object> { ["driver"] = "test" }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCapabilityDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-capability-v1\"", json);
        Assert.Contains("\"supportedTransforms\":[\"Scale\",\"Offset\"]", json);
        Assert.NotNull(roundtrip);
        Assert.Contains(CollectionProtocolType.Modbus, roundtrip!.SupportedProtocols);
        Assert.Contains(roundtrip.TaskCapabilities, task => task.TaskType == nameof(EdgeTaskType.HealthProbe));
        Assert.Contains(roundtrip.CompatibleContracts, contract => contract.ContractVersion == EdgeNodeContractVersions.EdgeTaskV1);
    }

    [Fact]
    public void EdgeTaskDto_SerializesEnumsAsStableStrings()
    {
        var dto = new EdgeTaskRequestDto
        {
            TaskId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TaskType = EdgeTaskType.HealthProbe,
            CreatedAt = DateTime.Parse("2026-07-05T00:00:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.EdgeNode,
                DeviceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                TargetKey = "22222222-2222-2222-2222-222222222222:gateway"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-task-v1\"", json);
        Assert.Contains("\"taskType\":\"HealthProbe\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
    }

    [Fact]
    public void EdgeTaskStateDto_SerializesCurrentStatusAsStableString()
    {
        var dto = new EdgeTaskDto
        {
            TaskId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TaskType = EdgeTaskType.HealthProbe,
            Status = EdgeTaskStatus.Running,
            Progress = 25,
            CreatedAt = DateTime.Parse("2026-07-05T00:00:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.EdgeNode,
                DeviceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                TargetKey = "22222222-2222-2222-2222-222222222222:gateway"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-task-v1\"", json);
        Assert.Contains("\"taskType\":\"HealthProbe\"", json);
        Assert.Contains("\"status\":\"Running\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
    }

    [Fact]
    public void EdgeHeartbeatDto_ToleratesFutureFields()
    {
        const string json = """
        {
          "contractVersion": "edge-v1",
          "status": "Running",
          "healthy": true,
          "futureField": "ignored"
        }
        """;

        var dto = JsonSerializer.Deserialize<EdgeHeartbeatDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(dto);
        Assert.Equal(EdgeNodeStatusNames.Running, dto!.Status);
        Assert.True(dto.Healthy);
    }

    [Fact]
    public void EdgeTaskStateMachine_RejectsInvalidRollback()
    {
        Assert.True(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Pending, EdgeTaskStatus.Sent));
        Assert.True(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Running, EdgeTaskStatus.Succeeded));
        Assert.False(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Succeeded, EdgeTaskStatus.Running));
    }
}
