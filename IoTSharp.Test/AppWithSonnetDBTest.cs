#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Protocol;
using Quartz;
using Quartz.Impl.Matchers;
using SonnetDB.Data.Mq;
using Xunit;
using DataType = IoTSharp.Contracts.DataType;

namespace IoTSharp.Test;

[Collection(IntegrationTestCollectionNames.SonnetDbApplication)]
public sealed class AppWithSonnetDBTest
    : IoTSharpBusinessTestSuite<SonnetDbAppFixture>
{
    public AppWithSonnetDBTest(SonnetDbAppFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task SonnetDB_EndToEnd_CoversInstallDevicesTelemetryAttributesRulesAndProducts()
    {
        using var client = Fixture.CreateClient();
        await Fixture.AuthorizeClientAsync(client);

        var product = await CreateProductAsync(client);
        var productId = product.Id;
        var productToken = product.ProductToken;

        var device = (await Fixture.CreateDeviceAsync(client, $"sonnet-device-{Guid.NewGuid():N}")).Data!;
        var productDevice = await CreateDeviceFromProductAsync(client, productId);
        var token = await Fixture.GetDeviceAccessTokenAsync(client, device.Id);

        await AssertDeviceListContainsAsync(client, device, offset: 0);
        await AssertDeviceListContainsAsync(client, device, offset: 1);

        await UploadTelemetryAndAssertLatestAndHistoryAsync(client, device.Id, token);
        await UploadAttributesAndAssertLatestAsync(client, device.Id, token);
        await AddEditAndRemoveServerAttributeAsync(client, device.Id, token);
        await RunRuleChainSmokeAsync(client, device.Id, token);
        await AssertProductMappingRoutesDataAsync(client, productId, productToken, productDevice.Id);

        await DeleteDeviceAsync(client, device.Id);
        await DeleteDeviceAsync(client, productDevice.Id);
        await DeleteProductAsync(client, productId);
    }

    [Fact]
    public async Task SonnetDB_RecoversQuartzJobsAndStoresLowercaseMqttUplink()
    {
        using var before = Fixture.CreateClient();
        await Fixture.AuthorizeClientAsync(before);
        var device = (await Fixture.CreateDeviceAsync(before)).Data!;
        var token = await Fixture.GetDeviceAccessTokenAsync(before, device.Id);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(50));
        var ct = timeout.Token;

        var scheduler = await Fixture.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(ct);
        Assert.True((await scheduler.GetMetaData(ct)).JobStoreSupportsPersistence);
        var registered = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), ct);
        var triggers = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup(), ct);
        Assert.Contains(registered, key => key.Name == "CheckDevices");
        Assert.Contains(registered, key => key.Name == "CachingJob");
        var triggerKey = Assert.Single(triggers, key => key.Name == "CheckDevices");
        await scheduler.PauseTrigger(triggerKey, ct);

        await Fixture.RestartApplicationAsync();
        scheduler = await Fixture.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(ct);
        Assert.True(registered.ToHashSet().SetEquals(await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), ct)));
        Assert.True(triggers.ToHashSet().SetEquals(await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup(), ct)));
        Assert.Equal(TriggerState.Paused, await scheduler.GetTriggerState(triggerKey, ct));
        await scheduler.ResumeTrigger(triggerKey, ct);

        const string telemetryTopic = "iotsharp.services.datastream.telemetrydata";
        var queue = Fixture.Services.GetRequiredService<SndbMqClient>();
        var beforePublish = await queue.GetStatsAsync(telemetryTopic, ct);
        using var mqtt = new MqttClientFactory().CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithClientId("iotsharp-test-" + Guid.NewGuid().ToString("N"))
            .WithTcpServer("127.0.0.1", Fixture.Profile!.MqttBrokerPort)
            .WithCredentials(token, string.Empty)
            .WithCleanSession()
            .Build();
        var connection = await mqtt.ConnectAsync(options, ct);
        Assert.Equal(MqttClientConnectResultCode.Success, connection.ResultCode);

        var telemetryKey = "mqttTemperature_" + Guid.NewGuid().ToString("N")[..8];
        var alarmType = "mqtt-alarm-" + Guid.NewGuid().ToString("N");
        const double temperature = 22.75;
        await PublishMqttJsonAsync(mqtt, "devices/me/telemetry", new Dictionary<string, double> { [telemetryKey] = temperature }, ct);
        await PublishMqttJsonAsync(mqtt, "devices/me/attributes", new { mqttFirmware = "test-v1" }, ct);
        await PublishMqttJsonAsync(mqtt, "devices/me/alarm", new CreateAlarmDto
        {
            OriginatorType = OriginatorType.Device,
            AlarmType = alarmType,
            AlarmDetail = "MQTT integration test",
            Serverity = ServerityLevel.Warning
        }, ct);

        using var after = Fixture.CreateClient();
        await Fixture.AuthorizeClientAsync(after);
        await WaitForAsync(
            () => GetApiResultAsync<List<TelemetryDataDto>>(after, $"/api/Devices/{device.Id}/TelemetryLatest/{telemetryKey}"),
            result => result.Data?.Any(item => item.KeyName == telemetryKey && Math.Abs(ToDouble(item.Value) - temperature) < 0.001) == true,
            "MQTT telemetry latest");
        await WaitForAsync(
            () => GetApiResultAsync<List<AttributeDataDto>>(after, $"/api/Devices/{device.Id}/AttributeLatest/mqttFirmware"),
            result => result.Data?.Any(item => item.KeyName == "mqttFirmware" && item.Value?.ToString() == "test-v1") == true,
            "MQTT attributes latest");
        await WaitForAsync(
            () => PostApiResultAsync<PagedData<AlarmDto>>(after, "/api/alarm/list", new
            {
                offset = 0, limit = 10, AlarmStatus = -1, Serverity = -1, OriginatorType = (int)OriginatorType.Device, AlarmType = alarmType
            }),
            result => result.Data?.rows.Any(row => row.OriginatorId == device.Id) == true,
            "MQTT alarm persisted by device name");
        var afterPublish = await queue.GetStatsAsync(telemetryTopic, ct);
        Assert.True(afterPublish.NextOffset > beforePublish.NextOffset);
        await mqtt.DisconnectAsync(cancellationToken: ct);
        await DeleteDeviceAsync(after, device.Id);
    }

    [Fact]
    public async Task SonnetDB_AlarmAndRuleAuditLists_AcceptOmittedFilters()
    {
        using var client = Fixture.CreateClient();
        await Fixture.AuthorizeClientAsync(client);
        var device = (await Fixture.CreateDeviceAsync(client)).Data!;
        var alarmType = "sonnet-alarm-" + Guid.NewGuid().ToString("N");
        using var response = await client.PostAsJsonAsync("/api/alarm/occurred", new CreateAlarmDto
        {
            OriginatorName = device.Id.ToString("D").ToUpperInvariant(),
            OriginatorType = OriginatorType.Device,
            AlarmType = alarmType,
            AlarmDetail = "Alarm list integration test",
            Serverity = ServerityLevel.Warning
        });
        await AssertApiSuccessAsync<Alarm>(response);

        // 模拟列表默认请求和仅选择来源类型的请求，均省略可空 Guid 与日期字段。
        var unfiltered = await PostApiResultAsync<PagedData<AlarmDto>>(client, "/api/alarm/list", new
        {
            offset = 0, limit = 10, AlarmStatus = -1, Serverity = -1, OriginatorType = -1, AlarmType = alarmType
        });
        Assert.Contains(unfiltered.Data!.rows, row => row.OriginatorId == device.Id);

        var byOriginatorType = await PostApiResultAsync<PagedData<AlarmDto>>(client, "/api/alarm/list", new
        {
            offset = 0, limit = 10, AlarmStatus = -1, Serverity = -1, OriginatorType = (int)OriginatorType.Device, AlarmType = alarmType
        });
        Assert.Contains(byOriginatorType.Data!.rows, row => row.OriginatorId == device.Id);

        var audit = await PostApiResultAsync<PagedData<BaseEventDto>>(client, "/api/rules/flowevents", new
        {
            offset = 0, limit = 10, Name = string.Empty
        });
        Assert.NotNull(audit.Data);
        await DeleteDeviceAsync(client, device.Id);
    }

    private static async Task PublishMqttJsonAsync(IMqttClient mqtt, string topic, object payload, CancellationToken cancellationToken)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.SerializeToUtf8Bytes(payload))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        var result = await mqtt.PublishAsync(message, cancellationToken);
        Assert.True(result.IsSuccess, result.ReasonString);
    }

    private static async Task<ProductAddDto> CreateProductAsync(HttpClient client)
    {
        var productToken = "prod-" + Guid.NewGuid().ToString("N");
        var save = await client.PostAsJsonAsync("/api/Products/Save", new ProductAddDto
        {
            Name = "sonnet-product-" + Guid.NewGuid().ToString("N"),
            Description = "SonnetDB e2e product",
            ProductToken = productToken,
            DefaultDeviceType = DeviceType.Device,
            DefaultIdentityType = IdentityType.AccessToken,
            DefaultTimeout = 45,
            GatewayConfiguration = string.Empty
        });
        await AssertApiSuccessAsync<bool>(save);

        var list = await GetApiResultAsync<PagedData<ProductDto>>(client, "/api/Products/List?offset=0&limit=10&name=sonnet-product");
        Assert.NotNull(list.Data);
        var row = Assert.Single(list.Data!.rows.Where(item => item.Name.StartsWith("sonnet-product-", StringComparison.Ordinal)));

        var defaults = await client.PostAsJsonAsync("/api/Products/EditProductData", new ProductDataEditDto
        {
            ProductId = row.Id,
            ProductData =
            [
                new ProductDataItemDto { KeyName = "defaultMode", DataSide = DataSide.ServerSide, Type = DataType.String },
                new ProductDataItemDto { KeyName = "productAttr", DataSide = DataSide.ClientSide, Type = DataType.String },
                new ProductDataItemDto { KeyName = "productTemperature", DataSide = DataSide.ClientSide, Type = DataType.Double }
            ]
        });
        await AssertApiSuccessAsync<bool>(defaults);

        return new ProductAddDto
        {
            Id = row.Id,
            Name = row.Name,
            ProductToken = productToken,
            DefaultDeviceType = row.DefaultDeviceType,
            DefaultIdentityType = row.DefaultIdentityType,
            DefaultTimeout = row.DefaultTimeout,
            Description = row.Description
        };
    }

    private static async Task<Device> CreateDeviceFromProductAsync(HttpClient client, Guid productId)
    {
        var response = await client.PostAsJsonAsync($"/api/Devices/product/{productId}", new DevicePostProductDto
        {
            Name = "sonnet-product-device-" + Guid.NewGuid().ToString("N")
        });
        var result = await AssertApiSuccessAsync<Device>(response);
        Assert.NotNull(result.Data);
        return result.Data!;
    }

    private static async Task AssertDeviceListContainsAsync(HttpClient client, Device device, int offset)
    {
        var url = $"/api/Devices/Customers?customerId={device.Customer.Id}&offset={offset}&limit=5";
        var result = await GetApiResultAsync<PagedData<DeviceDetailDto>>(client, url);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.total >= 1);
        if (offset == 0)
        {
            Assert.Contains(result.Data.rows, row => row.Id == device.Id);
        }
    }

    private static async Task UploadTelemetryAndAssertLatestAndHistoryAsync(HttpClient client, Guid deviceId, string token)
    {
        var key = "temperature_" + Guid.NewGuid().ToString("N")[..8];
        var value = 23.75;
        var response = await client.PostAsJsonAsync($"/api/Devices/{token}/Telemetry", new Dictionary<string, object>
        {
            [key] = value
        });
        await AssertApiSuccessAsync<Dictionary<string, string>>(response);

        var latest = await WaitForAsync(
            () => GetApiResultAsync<List<TelemetryDataDto>>(client, $"/api/Devices/{deviceId}/TelemetryLatest/{key}"),
            result => result.Data?.Any(item => item.KeyName == key && Math.Abs(ToDouble(item.Value) - value) < 0.001) == true,
            $"telemetry latest '{key}'");
        Assert.Contains(latest.Data!, item => item.KeyName == key);

        var begin = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-5).ToString("O"));
        var end = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(5).ToString("O"));
        var history = await WaitForAsync(
            () => GetApiResultAsync<List<TelemetryDataDto>>(client, $"/api/Devices/{deviceId}/TelemetryData/{key}/{begin}/{end}"),
            result => result.Data?.Any(item => item.KeyName == key && Math.Abs(ToDouble(item.Value) - value) < 0.001) == true,
            $"telemetry history '{key}'");
        Assert.Contains(history.Data!, item => item.KeyName == key);
    }

    private static async Task UploadAttributesAndAssertLatestAsync(HttpClient client, Guid deviceId, string token)
    {
        var key = "firmware_" + Guid.NewGuid().ToString("N")[..8];
        var value = "1.2." + Random.Shared.Next(10, 99);
        var response = await client.PostAsJsonAsync($"/api/Devices/{token}/Attributes", new Dictionary<string, object>
        {
            [key] = value
        });
        await AssertApiSuccessAsync<object>(response);

        var latest = await WaitForAsync(
            () => GetApiResultAsync<List<AttributeDataDto>>(client, $"/api/Devices/{deviceId}/AttributeLatest/{key}"),
            result => result.Data?.Any(item => item.KeyName == key && item.Value?.ToString() == value) == true,
            $"attribute latest '{key}'");
        Assert.Contains(latest.Data!, item => item.KeyName == key);
    }

    private static async Task AddEditAndRemoveServerAttributeAsync(HttpClient client, Guid deviceId, string token)
    {
        var key = "serverThreshold_" + Guid.NewGuid().ToString("N")[..8];
        var add = await client.PostAsJsonAsync($"/api/Devices/{token}/AddAttribute", new DeviceAttributeDto
        {
            DeviceId = deviceId,
            KeyName = key,
            DataSide = DataSide.ServerSide,
            DataType = DataType.Double,
            Catalog = DataCatalog.AttributeLatest,
            DateTime = DateTime.UtcNow
        });
        await AssertApiSuccessAsync<bool>(add);

        var edit = await client.PostAsJsonAsync($"/api/Devices/{deviceId}/EditAttribute", new DeviceAttrEditDto
        {
            serverside = new Dictionary<string, object> { [key] = 88.5 }
        });
        await AssertApiSuccessAsync<Dictionary<string, string>>(edit);

        var latest = await WaitForAsync(
            () => GetApiResultAsync<List<AttributeDataDto>>(client, $"/api/Devices/{deviceId}/AttributeLatest/{key}"),
            result => result.Data?.Any(item => item.KeyName == key && Math.Abs(ToDouble(item.Value) - 88.5) < 0.001) == true,
            $"server attribute '{key}'");
        Assert.Contains(latest.Data!, item => item.KeyName == key);

        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Devices/RemoveAttribute")
        {
            Content = JsonContent.Create(new RemoveDeviceAttributeInput
            {
                DeviceId = deviceId,
                KeyName = key,
                DataSide = DataSide.ServerSide
            })
        };
        var remove = await client.SendAsync(request);
        await AssertApiSuccessAsync<bool>(remove);
    }

    private static async Task RunRuleChainSmokeAsync(HttpClient client, Guid deviceId, string token)
    {
        var ruleId = Guid.NewGuid();
        var saveRule = await client.PostAsJsonAsync("/api/Rules/Save", new FlowRule
        {
            RuleId = ruleId,
            RuleType = RuleType.RuleNode,
            Name = "sonnet-raw-rule-" + Guid.NewGuid().ToString("N"),
            Describes = "SonnetDB raw rule smoke",
            Runner = "builtin",
            ExecutableCode = "{}",
            RuleDesc = "SonnetDB raw rule smoke",
            DefinitionsXml = "<rule />",
            Version = 1,
            SubVersion = 0,
            MountType = EventType.RAW
        });
        await AssertApiSuccessAsync<bool>(saveRule);

        var bind = await client.PostAsJsonAsync("/api/Rules/BindDevice", new ModelRuleBind
        {
            rule = ruleId,
            dev = [deviceId]
        });
        await AssertApiSuccessAsync<bool>(bind);

        var body = new StringContent("{\"raw\":true}", Encoding.UTF8, "application/json");
        var trigger = await client.PostAsync($"/api/Devices/{token}/PushDataToRuleChains/json", body);
        await AssertApiSuccessAsync<object>(trigger);

        var events = await WaitForAsync(
            () => PostApiResultAsync<PagedData<BaseEventDto>>(client, "/api/Rules/FlowEvents", new EventParam
            {
                RuleId = ruleId,
                Offset = 0,
                Limit = 10
            }),
            result => result.Data?.rows.Any(item => item.RuleId == ruleId) == true,
            "rule chain event");
        Assert.Contains(events.Data!.rows, item => item.RuleId == ruleId);
    }

    private static async Task AssertProductMappingRoutesDataAsync(HttpClient client, Guid productId, string productToken, Guid deviceId)
    {
        var saveMapping = await client.PostAsJsonAsync("/api/Products/SaveDataMappings", new SaveProductDataMappingsDto
        {
            ProductId = productId,
            Mappings =
            [
                new ProductDataMappingDto
                {
                    ProductKeyName = "productTemperature",
                    DataCatalog = DataCatalog.TelemetryData,
                    DeviceId = deviceId,
                    DeviceKeyName = "mappedTemperature",
                    Description = "telemetry mapping"
                },
                new ProductDataMappingDto
                {
                    ProductKeyName = "productAttr",
                    DataCatalog = DataCatalog.AttributeData,
                    DeviceId = deviceId,
                    DeviceKeyName = "mappedAttr",
                    Description = "attribute mapping"
                }
            ]
        });
        await AssertApiSuccessAsync<bool>(saveMapping);

        var telemetry = await client.PostAsJsonAsync($"/api/Products/{productToken}/Telemetry", new Dictionary<string, object>
        {
            ["productTemperature"] = 31.25
        });
        await AssertApiSuccessAsync<object>(telemetry);

        await WaitForAsync(
            () => GetApiResultAsync<List<TelemetryDataDto>>(client, $"/api/Devices/{deviceId}/TelemetryLatest/mappedTemperature"),
            result => result.Data?.Any(item => item.KeyName == "mappedTemperature" && Math.Abs(ToDouble(item.Value) - 31.25) < 0.001) == true,
            "product telemetry mapping");

        var attr = await client.PostAsJsonAsync($"/api/Products/{productToken}/Attributes", new Dictionary<string, object>
        {
            ["productAttr"] = "mapped-ok"
        });
        await AssertApiSuccessAsync<object>(attr);

        var productAttrs = await WaitForAsync(
            () => GetApiResultAsync<JsonElement>(client, $"/api/Products/{productToken}/Attributes/{DataSide.ClientSide}?keys=productAttr"),
            result => ApiSucceeded(result)
                && result.Data.ValueKind == JsonValueKind.Array
                && result.Data.EnumerateArray().Any(item =>
                    item.TryGetProperty("productKeyName", out var productKeyName)
                    && productKeyName.GetString() == "productAttr"
                    && item.TryGetProperty("deviceKeyName", out var deviceKeyName)
                    && deviceKeyName.GetString() == "mappedAttr"
                    && item.TryGetProperty("value", out var value)
                    && value.GetString() == "mapped-ok"),
            "product attribute mapping");
        Assert.Equal((int)ApiCode.Success, productAttrs.Code);
    }

    private static async Task DeleteDeviceAsync(HttpClient client, Guid deviceId)
    {
        var response = await client.DeleteAsync($"/api/Devices/{deviceId}");
        await AssertApiSuccessAsync<bool>(response);
    }

    private static async Task DeleteProductAsync(HttpClient client, Guid productId)
    {
        var response = await client.GetAsync($"/api/Products/Delete?productId={productId}");
        await AssertApiSuccessAsync<bool>(response);
    }

    private static async Task<ApiResult<T>> GetApiResultAsync<T>(HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        return await AssertApiSuccessAsync<T>(response);
    }

    private static async Task<ApiResult<T>> PostApiResultAsync<T>(HttpClient client, string requestUri, object payload)
    {
        var response = await client.PostAsJsonAsync(requestUri, payload);
        return await AssertApiSuccessAsync<T>(response);
    }

    private static async Task<ApiResult<T>> AssertApiSuccessAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        var result = JsonSerializer.Deserialize<ApiResult<T>>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(result);
        Assert.True(ApiSucceeded(result!), $"API failed. Code={result!.Code}, Msg={result.Msg}, Body={body}");
        return result;
    }

    private static bool ApiSucceeded<T>(ApiResult<T> result)
        => result.Code == (int)ApiCode.Success || result.Code == 0;

    private static async Task<ApiResult<T>> WaitForAsync<T>(
        Func<Task<ApiResult<T>>> action,
        Func<ApiResult<T>, bool> predicate,
        string description)
    {
        ApiResult<T>? last = null;
        for (var attempt = 0; attempt < 40; attempt++)
        {
            last = await action();
            if (predicate(last))
            {
                return last;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        Assert.Fail($"Timed out waiting for {description}. Last Code={last?.Code}, Msg={last?.Msg}");
        throw new UnreachableException();
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

    private sealed class UnreachableException : Exception;
}
