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
using Xunit;

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

        var produceName = $"product-key-{Guid.NewGuid():N}";
        var produceToken = $"pk-{Guid.NewGuid():N}";
        var saveProduce = await client.PostAsJsonAsync("/api/Produces/Save", new ProduceAddDto
        {
            Name = produceName,
            Description = "product key identity test",
            ProduceToken = produceToken,
            DefaultDeviceType = DeviceType.Device,
            DefaultIdentityType = IdentityType.ProduceToken,
            DefaultTimeout = 30,
            GatewayConfiguration = string.Empty
        });
        var saved = await ReadApiResultAsync<bool>(saveProduce);
        Assert.Equal((int)ApiCode.Success, saved.Code);

        var listed = await GetApiResultAsync<PagedData<ProduceDto>>(client,
            $"/api/Produces/List?offset=0&limit=10&name={Uri.EscapeDataString(produceName)}");
        var produce = Assert.Single(listed.Data!.rows, p => p.Name == produceName);

        var deviceName = $"product-device-{Guid.NewGuid():N}";
        var created = await client.PostAsJsonAsync($"/api/Devices/produce/{produce.Id}", new DevicePostProduceDto
        {
            Name = deviceName
        });
        var createdDevice = await ReadApiResultAsync<Device>(created);
        Assert.Equal((int)ApiCode.Success, createdDevice.Code);

        var detail = await Fixture.GetDeviceDetailAsync(client, createdDevice.Data!.Id);
        Assert.Equal(IdentityType.ProduceToken, detail.Data!.IdentityType);
        Assert.Equal(deviceName, detail.Data.IdentityId);
        Assert.Equal(produceToken, detail.Data.IdentityValue);
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
            Protocols = ["modbus"],
            Features = ["collection"],
            Tasks = ["poll"],
            Metadata = new Dictionary<string, object> { ["driver"] = "test" }
        });
        await ReadApiResultAsync<object>(capabilities);

        await Fixture.AuthorizeClientAsync(client);
        var listed = await GetApiResultAsync<PagedData<EdgeNodeDto>>(client, "/api/Edge?limit=10");
        Assert.NotNull(listed);
        Assert.Equal((int)ApiCode.Success, listed!.Code);
        Assert.NotNull(listed.Data);
        Assert.Contains(listed.Data!.rows, x => x.Id == deviceId && x.Status == "Running" && x.Healthy == true);

        var pulled = await GetApiResultAsync<EdgeCollectionConfigurationDto>(client, $"/api/Edge/{deviceId}/CollectionConfig");
        Assert.NotNull(pulled);
        Assert.Equal((int)ApiCode.Success, pulled!.Code);
        Assert.NotNull(pulled.Data);
        Assert.Equal(deviceId, pulled.Data!.EdgeNodeId);
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
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");

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
