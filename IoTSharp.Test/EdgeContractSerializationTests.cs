#nullable enable

using System;
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
        Assert.Equal(EdgeNodeContractVersions.EdgeRuntimeV1, new EdgeRegistrationDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeNodeV1, new EdgeNodeDto().ContractVersion);
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
