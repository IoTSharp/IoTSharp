#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EdgeCapabilityDto = IoTSharp.Contracts.EdgeCapabilityDto;
using EdgeCapabilityReportDto = IoTSharp.Contracts.EdgeCapabilityReportDto;
using EdgeHeartbeatDto = IoTSharp.Contracts.EdgeHeartbeatDto;
using EdgeNodeDto = IoTSharp.Contracts.EdgeNodeDto;
using EdgeRegistrationDto = IoTSharp.Contracts.EdgeRegistrationDto;
using EdgeRegistrationResultDto = IoTSharp.Contracts.EdgeRegistrationResultDto;
using EdgeRuntimeStatusDto = IoTSharp.Contracts.EdgeRuntimeStatusDto;
using EdgeTaskAddressDto = IoTSharp.Contracts.EdgeTaskAddressDto;
using EdgeTaskCapabilityDto = IoTSharp.Contracts.EdgeTaskCapabilityDto;
using EdgeTaskReceiptDto = IoTSharp.Contracts.EdgeTaskReceiptDto;
using EdgeTaskRequestDto = IoTSharp.Contracts.EdgeTaskRequestDto;
using EdgeTaskStatus = IoTSharp.Contracts.EdgeTaskStatus;
using EdgeTaskTargetType = IoTSharp.Contracts.EdgeTaskTargetType;
using EdgeTaskType = IoTSharp.Contracts.EdgeTaskType;

namespace IoTSharp.Test;

public abstract class IoTSharpBusinessTestSuite<TFixture>
    where TFixture : AppInstance
{
    protected IoTSharpBusinessTestSuite(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [Fact]
    public Task AppIsInstalled() => Fixture.AssertAppIsInstalledAsync();

    [Fact]
    public Task AppAccountLogin() => Fixture.AssertAppAccountLoginAsync();

    [Fact]
    public async Task Devices_CanCreateReadUpdateAndDelete()
    {
        using var client = Fixture.CreateClient();
        var created = await Fixture.CreateDeviceAsync(client, $"device-crud-{Guid.NewGuid():N}");
        var device = created.Data!;

        var detail = await Fixture.GetDeviceDetailAsync(client, device.Id);
        Assert.Equal(device.Id, detail.Data!.Id);
        Assert.Equal(device.Name, detail.Data.Name);
        Assert.False(string.IsNullOrWhiteSpace(detail.Data.IdentityId));

        var updatedName = $"{device.Name}-updated";
        var put = await client.PutAsJsonAsync($"/api/Devices/{device.Id}", new DevicePutDto
        {
            Id = device.Id,
            Name = updatedName,
            DeviceType = DeviceType.Device,
            Timeout = 45,
            IdentityType = IdentityType.AccessToken
        });
        var putResult = await ReadApiResultAsync<bool>(put);
        Assert.NotNull(putResult);
        Assert.Equal((int)ApiCode.Success, putResult!.Code);
        Assert.True(putResult.Data);

        var afterUpdate = await Fixture.GetDeviceDetailAsync(client, device.Id);
        Assert.Equal(updatedName, afterUpdate.Data!.Name);
        Assert.Equal(45, afterUpdate.Data.Timeout);

        var delete = await client.DeleteAsync($"/api/Devices/{device.Id}");
        var deleteResult = await ReadApiResultAsync<bool>(delete);
        Assert.NotNull(deleteResult);
        Assert.Equal((int)ApiCode.Success, deleteResult!.Code);
        Assert.True(deleteResult.Data);
    }

    [Fact]
    public async Task Devices_CreatedFromProductKeyHaveManageableIdentity()
    {
        using var client = Fixture.CreateClient();
        await Fixture.AuthorizeClientAsync(client);

        var productName = $"product-key-{Guid.NewGuid():N}";
        var ProductToken = $"pk-{Guid.NewGuid():N}";
        var saveProduct = await client.PostAsJsonAsync("/api/Products/Save", new ProductAddDto
        {
            Name = productName,
            Description = "product key identity test",
            ProductToken = ProductToken,
            DefaultDeviceType = DeviceType.Device,
            DefaultIdentityType = IdentityType.ProductToken,
            DefaultTimeout = 30,
            GatewayConfiguration = string.Empty
        });
        var saved = await ReadApiResultAsync<bool>(saveProduct);
        Assert.Equal((int)ApiCode.Success, saved.Code);

        var listed = await GetApiResultAsync<PagedData<ProductDto>>(client,
            $"/api/Products/List?offset=0&limit=10&name={Uri.EscapeDataString(productName)}");
        var product = Assert.Single(listed.Data!.rows, p => p.Name == productName);

        var deviceName = $"product-device-{Guid.NewGuid():N}";
        var created = await client.PostAsJsonAsync($"/api/Devices/product/{product.Id}", new DevicePostProductDto
        {
            Name = deviceName
        });
        var createdDevice = await ReadApiResultAsync<Device>(created);
        Assert.Equal((int)ApiCode.Success, createdDevice.Code);

        var detail = await Fixture.GetDeviceDetailAsync(client, createdDevice.Data!.Id);
        Assert.Equal(IdentityType.ProductToken, detail.Data!.IdentityType);
        Assert.Equal(deviceName, detail.Data.IdentityId);
        Assert.Equal(ProductToken, detail.Data.IdentityValue);
    }

    [Fact]
    public async Task Telemetry_HttpIngestStoresLatestAndHistory()
    {
        using var client = Fixture.CreateClient();
        var created = await Fixture.CreateDeviceAsync(client, $"telemetry-{Guid.NewGuid():N}");
        var deviceId = created.Data!.Id;
        var token = await Fixture.GetDeviceAccessTokenAsync(client, deviceId);

        var response = await client.PostAsJsonAsync($"/api/Devices/{token}/Telemetry", new Dictionary<string, object>
        {
            ["temperature"] = 22.5,
            ["humidity"] = 61
        });
        var ingest = await ReadApiResultAsync<Dictionary<string, string>>(response);
        Assert.NotNull(ingest);
        Assert.Equal((int)ApiCode.Success, ingest!.Code);

        var latest = await WaitForTelemetryLatestAsync(client, deviceId, "temperature,humidity");
        Assert.Contains(latest, x => x.KeyName == "temperature" && ToDouble(x.Value) == 22.5);
        Assert.Contains(latest, x => x.KeyName == "humidity" && ToDouble(x.Value) == 61);

        var historyResponse = await GetApiResultAsync<List<TelemetryDataDto>>(client,
            $"/api/Devices/{deviceId}/TelemetryData/temperature/{Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-5).ToString("O"))}/{Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(5).ToString("O"))}");

        Assert.NotNull(historyResponse);
        Assert.Equal((int)ApiCode.Success, historyResponse!.Code);
        Assert.NotNull(historyResponse.Data);
        Assert.Contains(historyResponse.Data, x => x.KeyName == "temperature" && ToDouble(x.Value) == 22.5);
    }

    [Fact]
    public async Task EdgeRuntime_CanRegisterHeartbeatReportCapabilitiesAndPullCollectionConfig()
    {
        using var client = Fixture.CreateClient();
        var created = await Fixture.CreateDeviceAsync(client, $"edge-{Guid.NewGuid():N}", DeviceType.Gateway);
        var deviceId = created.Data!.Id;
        var token = await Fixture.GetDeviceAccessTokenAsync(client, deviceId);

        var register = await client.PostAsJsonAsync($"/api/Edge/{token}/Register", new EdgeRegistrationDto
        {
            RuntimeType = "iotedge-csharp-aot",
            RuntimeName = "test-edge",
            Version = "1.0.0-test",
            InstanceId = Guid.NewGuid().ToString("N"),
            Platform = "test",
            HostName = "test-host",
            IpAddress = "127.0.0.1",
            Metadata = new Dictionary<string, string> { ["profile"] = Fixture.Profile?.Name ?? "unknown" }
        });
        var registerResult = await ReadApiResultAsync<EdgeRegistrationResultDto>(register);
        Assert.NotNull(registerResult);
        Assert.Equal((int)ApiCode.Success, registerResult!.Code);
        Assert.Equal(deviceId, registerResult.Data!.DeviceId);
        Assert.Equal(deviceId, registerResult.Data.EdgeNodeId);
        Assert.Equal(deviceId, registerResult.Data.GatewayId);

        var heartbeat = await client.PostAsJsonAsync($"/api/Edge/{token}/Heartbeat", new EdgeHeartbeatDto
        {
            Status = "Running",
            Healthy = true,
            UptimeSeconds = 123,
            IpAddress = "127.0.0.2",
            Metrics = new Dictionary<string, object> { ["cpu"] = 0.25 }
        });
        await ReadApiResultAsync<object>(heartbeat);

        var capabilities = await client.PostAsJsonAsync($"/api/Edge/{token}/Capabilities", new EdgeCapabilityReportDto
        {
            ContractVersion = EdgeNodeContractVersions.EdgeCapabilityV1,
            Protocols = ["modbus"],
            SupportedProtocols = [CollectionProtocolType.Modbus],
            SupportedPointTypes = ["holding-register"],
            SupportedTransforms = [CollectionTransformType.Scale],
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
        });
        await ReadApiResultAsync<object>(capabilities);

        await Fixture.AuthorizeClientAsync(client);
        var listed = await GetApiResultAsync<PagedData<EdgeNodeDto>>(client, "/api/Edge?limit=10");
        Assert.NotNull(listed);
        Assert.Equal((int)ApiCode.Success, listed!.Code);
        Assert.NotNull(listed.Data);
        var edgeRow = Assert.Single(listed.Data!.rows, x => x.Id == deviceId);
        Assert.Equal("Running", edgeRow.Status);
        Assert.True(edgeRow.Healthy);
        Assert.Equal("Running", edgeRow.RuntimeStatus.Status);
        Assert.True(edgeRow.RuntimeStatus.Healthy);
        Assert.Equal(EdgeNodeContractVersions.EdgeCapabilityV1, edgeRow.Capability.ContractVersion);
        Assert.Contains("modbus", edgeRow.Capability.Protocols);
        Assert.Contains(CollectionProtocolType.Modbus, edgeRow.Capability.SupportedProtocols);
        Assert.Contains("holding-register", edgeRow.Capability.SupportedPointTypes);
        Assert.Contains(CollectionTransformType.Scale, edgeRow.Capability.SupportedTransforms);
        Assert.Contains(edgeRow.Capability.TaskCapabilities, task => task.TaskType == nameof(EdgeTaskType.HealthProbe));

        var runtimeStatus = await GetApiResultAsync<EdgeRuntimeStatusDto>(client, $"/api/Edge/{deviceId}/RuntimeStatus");
        Assert.NotNull(runtimeStatus);
        Assert.Equal((int)ApiCode.Success, runtimeStatus!.Code);
        Assert.NotNull(runtimeStatus.Data);
        Assert.Equal(deviceId, runtimeStatus.Data!.EdgeNodeId);
        Assert.Equal(deviceId, runtimeStatus.Data.GatewayId);
        Assert.Equal("Running", runtimeStatus.Data.Status);
        Assert.Equal("127.0.0.2", runtimeStatus.Data.IpAddress);
        Assert.True(runtimeStatus.Data.Healthy);
        Assert.Equal(123, runtimeStatus.Data.UptimeSeconds);
        Assert.True(runtimeStatus.Data.Metrics.ContainsKey("cpu"));

        var capability = await GetApiResultAsync<EdgeCapabilityDto>(client, $"/api/Edge/{deviceId}/Capability");
        Assert.NotNull(capability);
        Assert.Equal((int)ApiCode.Success, capability!.Code);
        Assert.NotNull(capability.Data);
        Assert.Equal(deviceId, capability.Data!.EdgeNodeId);
        Assert.Equal(deviceId, capability.Data.GatewayId);
        Assert.Equal(EdgeNodeContractVersions.EdgeCapabilityV1, capability.Data.ContractVersion);
        Assert.Contains("collection", capability.Data.Features);
        Assert.Contains(capability.Data.CompatibleContracts, contract => contract.ContractVersion == EdgeNodeContractVersions.EdgeTaskV1);

        var templateId = Guid.NewGuid();
        var saveConfig = await client.PutAsJsonAsync($"/api/Edge/{deviceId}/CollectionConfig", new EdgeCollectionConfigurationUpdateDto
        {
            SourceType = "ProductCollectionTemplate",
            SourceId = templateId.ToString("D"),
            SourceVersion = "7",
            SourceMetadata = new Dictionary<string, object>
            {
                ["templateKey"] = "boiler-modbus-template",
                ["productId"] = Guid.NewGuid().ToString("D")
            },
            Tasks =
            [
                new CollectionTaskDto
                {
                    TaskKey = "boiler-modbus",
                    Protocol = CollectionProtocolType.Modbus,
                    Connection = new CollectionConnectionDto
                    {
                        ConnectionKey = "main-plc",
                        ConnectionName = "Main PLC",
                        Protocol = CollectionProtocolType.Modbus,
                        Transport = "tcp",
                        Host = "127.0.0.1",
                        Port = 1502
                    },
                    Devices =
                    [
                        new CollectionDeviceDto
                        {
                            DeviceKey = "boiler-01",
                            DeviceName = "Boiler 01",
                            Points =
                            [
                                new CollectionPointDto
                                {
                                    PointKey = "supply-temperature",
                                    PointName = "Supply temperature",
                                    SourceType = "holding-register",
                                    Address = "40001",
                                    RawValueType = "Int16",
                                    Length = 1,
                                    Polling = new PollingPolicyDto { ReadPeriodMs = 1000 },
                                    Transforms =
                                    [
                                        new ValueTransformDto
                                        {
                                            TransformType = CollectionTransformType.Scale,
                                            Order = 0,
                                            Parameters = JsonSerializer.SerializeToElement(new Dictionary<string, object>
                                            {
                                                ["factor"] = 0.1
                                            })
                                        }
                                    ],
                                    Mapping = new PlatformMappingDto
                                    {
                                        TargetType = CollectionTargetType.Telemetry,
                                        TargetName = "supplyTemperature",
                                        ValueType = CollectionValueType.Double
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        });
        var savedConfig = await ReadApiResultAsync<EdgeCollectionConfigurationDto>(saveConfig);
        Assert.Equal((int)ApiCode.Success, savedConfig.Code);
        Assert.NotNull(savedConfig.Data);
        Assert.Equal("ProductCollectionTemplate", savedConfig.Data!.SourceType);
        Assert.Equal(templateId.ToString("D"), savedConfig.Data.SourceId);

        var pulled = await GetApiResultAsync<EdgeCollectionConfigurationDto>(client, $"/api/Edge/{deviceId}/CollectionConfig");
        Assert.NotNull(pulled);
        Assert.Equal((int)ApiCode.Success, pulled!.Code);
        Assert.NotNull(pulled.Data);
        Assert.Equal(deviceId, pulled.Data!.EdgeNodeId);
        Assert.Equal("ProductCollectionTemplate", pulled.Data.SourceType);
        Assert.Equal(templateId.ToString("D"), pulled.Data.SourceId);
        Assert.Equal("7", pulled.Data.SourceVersion);

        var assignments = await GetApiResultAsync<PagedData<EdgeCollectionAssignmentDto>>(client, $"/api/Edge/{deviceId}/CollectionAssignments?limit=10");
        Assert.NotNull(assignments);
        Assert.Equal((int)ApiCode.Success, assignments!.Code);
        var activeAssignment = Assert.Single(assignments.Data!.rows, item => item.Status == EdgeCollectionAssignmentStatus.Active);
        Assert.Equal("ProductCollectionTemplate", activeAssignment.SourceType);
        Assert.Equal(templateId.ToString("D"), activeAssignment.SourceId);
    }

    [Fact]
    public async Task EdgeTask_CanDispatchPullAcceptAndRejectInvalidTransition()
    {
        using var client = Fixture.CreateClient();
        var created = await Fixture.CreateDeviceAsync(client, $"edge-task-{Guid.NewGuid():N}", DeviceType.Gateway);
        var deviceId = created.Data!.Id;
        var token = await Fixture.GetDeviceAccessTokenAsync(client, deviceId);
        var taskId = Guid.NewGuid();

        await Fixture.AuthorizeClientAsync(client);
        var dispatch = await client.PostAsJsonAsync("/api/EdgeTask/Dispatch", new EdgeTaskRequestDto
        {
            ContractVersion = "edge-task-v1",
            TaskId = taskId,
            TaskType = EdgeTaskType.HealthProbe,
            CreatedAt = DateTime.UtcNow,
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.EdgeNode,
                DeviceId = deviceId,
                RuntimeType = "gateway",
                TargetKey = deviceId.ToString()
            },
            Parameters = new Dictionary<string, object> { ["probe"] = "ping" },
            Metadata = new Dictionary<string, string> { ["source"] = "test" }
        });
        var dispatchResult = await ReadApiResultAsync<EdgeTaskRequestDto>(dispatch);
        Assert.Equal((int)ApiCode.Success, dispatchResult.Code);

        using (var scope = Fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == taskId);
            Assert.Equal(deviceId, storedTask.GatewayId);
            Assert.Equal(EdgeTaskStatus.Pending, storedTask.Status);
            Assert.Equal(EdgeTaskType.HealthProbe, storedTask.TaskType);
        }

        var pulled = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
        Assert.Equal((int)ApiCode.Success, pulled.Code);
        Assert.Contains(pulled.Data!, task => task.TaskId == taskId);

        using (var scope = Fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == taskId);
            Assert.Equal(EdgeTaskStatus.Sent, storedTask.Status);
            Assert.NotNull(storedTask.SentAt);
        }

        var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
        {
            ContractVersion = "edge-task-v1",
            TaskId = taskId,
            TargetType = EdgeTaskTargetType.EdgeNode,
            TargetKey = deviceId.ToString(),
            RuntimeType = "gateway",
            Status = EdgeTaskStatus.Accepted,
            ReportedAt = DateTime.UtcNow
        });
        var acceptedResult = await ReadApiResultAsync<object>(accepted);
        Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

        var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
        {
            ContractVersion = "edge-task-v1",
            TaskId = taskId,
            TargetType = EdgeTaskTargetType.EdgeNode,
            TargetKey = deviceId.ToString(),
            RuntimeType = "gateway",
            Status = EdgeTaskStatus.Running,
            Progress = 25,
            ReportedAt = DateTime.UtcNow
        });
        var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
        Assert.Equal((int)ApiCode.Success, runningResult.Code);

        using (var scope = Fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == taskId);
            Assert.Equal(EdgeTaskStatus.Running, storedTask.Status);
            Assert.Equal(25, storedTask.Progress);
            Assert.NotNull(storedTask.AcceptedAt);
            Assert.NotNull(storedTask.StartedAt);
            Assert.NotNull(storedTask.LastReceiptAt);
        }

        var invalid = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
        {
            ContractVersion = "edge-task-v1",
            TaskId = taskId,
            TargetType = EdgeTaskTargetType.EdgeNode,
            TargetKey = deviceId.ToString(),
            RuntimeType = "gateway",
            Status = EdgeTaskStatus.Accepted,
            ReportedAt = DateTime.UtcNow
        });
        var invalidResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(invalid);
        Assert.Equal((int)ApiCode.InValidData, invalidResult.Code);
        Assert.Contains("Invalid edge task transition", invalidResult.Msg);

        var latest = await GetApiResultAsync<EdgeTaskReceiptDto>(client, $"/api/EdgeTask/Receipt/{deviceId}");
        Assert.Equal((int)ApiCode.Success, latest.Code);
        Assert.NotNull(latest.Data);
        Assert.Equal(EdgeTaskStatus.Running, latest.Data!.Status);
    }

    private static async Task<List<TelemetryDataDto>> WaitForTelemetryLatestAsync(HttpClient client, Guid deviceId, string keys)
    {
        ApiResult<List<TelemetryDataDto>>? result = null;

        for (var attempt = 0; attempt < 30; attempt++)
        {
            result = await GetApiResultAsync<List<TelemetryDataDto>>(client, $"/api/Devices/{deviceId}/TelemetryLatest/{keys}");
            if (result?.Data?.Any() == true)
            {
                return result.Data;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        Assert.NotNull(result);
        Assert.Equal((int)ApiCode.Success, result!.Code);
        Assert.NotNull(result.Data);
        return result.Data!;
    }

    private static async Task<ApiResult<T>> GetApiResultAsync<T>(HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        return await ReadApiResultAsync<T>(response);
    }

    private static async Task<ApiResult<T>> ReadApiResultAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var headers = string.Join("; ", response.Headers.Select(header => $"{header.Key}={string.Join(",", header.Value)}"));
        var contentHeaders = string.Join("; ", response.Content.Headers.Select(header => $"{header.Key}={string.Join(",", header.Value)}"));
        Assert.True(response.IsSuccessStatusCode, $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri} => {(int)response.StatusCode} {response.ReasonPhrase}; Headers=[{headers}]; ContentHeaders=[{contentHeaders}]: {body}");

        var result = await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        Assert.NotNull(result);
        return result!;
    }

    private static double ToDouble(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String
                ? double.Parse(element.GetString()!)
                : element.GetDouble();
        }

        return Convert.ToDouble(value);
    }

}
