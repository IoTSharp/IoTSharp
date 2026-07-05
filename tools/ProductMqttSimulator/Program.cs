using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTnet;
using MQTTnet.Protocol;

var options = SimulatorOptions.Parse(args);
if (options.ShowHelp)
{
    SimulatorOptions.PrintHelp();
    return 0;
}

using var cts = new CancellationTokenSource(options.Timeout);
var cancellationToken = cts.Token;

using var http = new HttpClient
{
    BaseAddress = options.ApiBase,
    Timeout = options.Timeout
};

if (!string.IsNullOrWhiteSpace(options.BearerToken))
{
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.BearerToken);
}

var productName = options.ProductName ?? $"mqtt-sim-product-{Guid.NewGuid():N}";
var deviceName = options.DeviceName ?? $"mqtt-sim-device-{Guid.NewGuid():N}";
var topicDeviceName = options.TopicDeviceName ?? "me";
var telemetryKey = options.TelemetryKey ?? "sim_temperature";
var telemetryValue = options.TelemetryValue ?? Math.Round(20 + Random.Shared.NextDouble() * 10, 2);
var productToken = options.ProductToken;
var createdProduct = string.IsNullOrWhiteSpace(productToken);
Guid? productId = null;

Console.WriteLine("IoTSharp Product MQTT Simulator");
Console.WriteLine($"API:  {http.BaseAddress}");
Console.WriteLine($"MQTT: {options.MqttHost}:{options.MqttPort}");
Console.WriteLine($"DeviceName: {deviceName}");
Console.WriteLine($"TelemetryTopic: devices/{topicDeviceName}/telemetry");

if (createdProduct)
{
    RequireBearer(options);
    productToken = "prod-" + Guid.NewGuid().ToString("N");
    productId = await CreateProductAsync(http, productName, productToken, cancellationToken);
    Console.WriteLine($"Created product: {productName}");
    Console.WriteLine($"ProductId:      {productId}");
    Console.WriteLine($"ProductToken:   {productToken}");
}
else
{
    Console.WriteLine("Using existing product token.");
}

await PublishTelemetryByMqttAsync(
    options.MqttHost,
    options.MqttPort,
    deviceName,
    topicDeviceName,
    productToken!,
    telemetryKey,
    telemetryValue,
    options.UseTls,
    cancellationToken);

Console.WriteLine($"Published telemetry: {telemetryKey}={telemetryValue.ToString(CultureInfo.InvariantCulture)}");

if (!string.IsNullOrWhiteSpace(options.BearerToken)
    && (createdProduct || !string.IsNullOrWhiteSpace(options.ProductName)))
{
    var deviceId = await WaitForProductDeviceAsync(http, productName, productToken!, deviceName, cancellationToken);
    Console.WriteLine($"Registered device:  {deviceId}");

    await WaitForLatestTelemetryAsync(http, deviceId, telemetryKey, telemetryValue, cancellationToken);
    Console.WriteLine("Telemetry verified through HTTP latest telemetry API.");
}
else
{
    Console.WriteLine("Skipped HTTP verification because --bearer was not provided, or an existing token was used without --product-name.");
}

return 0;

static async Task<Guid> CreateProductAsync(HttpClient http, string productName, string productToken, CancellationToken cancellationToken)
{
    var response = await http.PostAsJsonAsync("/api/Products/Save", new
    {
        name = productName,
        description = "Product MQTT simulator",
        productToken,
        defaultDeviceType = "Device",
        defaultIdentityType = "ProductToken",
        defaultTimeout = 45,
        gatewayConfiguration = string.Empty,
        gatewayType = "Unknow"
    }, AppJsonContext.JsonOptions, cancellationToken);
    await EnsureApiSuccessAsync<ApiResultBool>(response, cancellationToken);

    var product = await WaitForProductAsync(http, productName, productToken, cancellationToken);
    return product.Id;
}

static async Task<ProductRow> WaitForProductAsync(HttpClient http, string productName, string productToken, CancellationToken cancellationToken)
{
    ApiResult<PagedData<ProductRow>>? last = null;
    for (var attempt = 0; attempt < 40; attempt++)
    {
        last = await GetApiResultAsync<PagedData<ProductRow>>(
            http,
            $"/api/Products/List?offset=0&limit=10&name={Uri.EscapeDataString(productName)}",
            cancellationToken);

        var product = last.Data?.Rows.FirstOrDefault(row =>
            string.Equals(row.Name, productName, StringComparison.Ordinal)
            || string.Equals(row.ProductToken, productToken, StringComparison.Ordinal));
        if (product is not null)
        {
            if (string.IsNullOrWhiteSpace(product.ProductToken))
            {
                var detail = await GetApiResultAsync<ProductRow>(
                    http,
                    $"/api/Products/Get?id={product.Id}",
                    cancellationToken);
                return detail.Data ?? product;
            }

            return product;
        }

        await Task.Delay(500, cancellationToken);
    }

    throw new InvalidOperationException($"Timed out waiting for product. Last API code={last?.Code}, msg={last?.Msg}");
}

static async Task<Guid> WaitForProductDeviceAsync(HttpClient http, string productName, string productToken, string deviceName, CancellationToken cancellationToken)
{
    ApiResult<PagedData<ProductRow>>? last = null;
    for (var attempt = 0; attempt < 60; attempt++)
    {
        last = await GetApiResultAsync<PagedData<ProductRow>>(
            http,
            $"/api/Products/List?offset=0&limit=10&name={Uri.EscapeDataString(productName)}",
            cancellationToken);

        var product = last.Data?.Rows.FirstOrDefault(row =>
            string.Equals(row.Name, productName, StringComparison.Ordinal)
            || string.Equals(row.ProductToken, productToken, StringComparison.Ordinal));
        var device = product?.Devices.FirstOrDefault(row => string.Equals(row.Name, deviceName, StringComparison.Ordinal));
        if (device is not null)
        {
            return device.Id;
        }

        await Task.Delay(1000, cancellationToken);
    }

    throw new InvalidOperationException($"Timed out waiting for MQTT-created device. Last API code={last?.Code}, msg={last?.Msg}");
}

static async Task WaitForLatestTelemetryAsync(HttpClient http, Guid deviceId, string telemetryKey, double expectedValue, CancellationToken cancellationToken)
{
    ApiResult<List<TelemetryDataDto>>? last = null;
    for (var attempt = 0; attempt < 60; attempt++)
    {
        last = await GetApiResultAsync<List<TelemetryDataDto>>(
            http,
            $"/api/Devices/{deviceId}/TelemetryLatest/{Uri.EscapeDataString(telemetryKey)}",
            cancellationToken);

        var hit = last.Data?.Any(item =>
            string.Equals(item.KeyName, telemetryKey, StringComparison.Ordinal)
            && TryReadDouble(item.Value, out var actual)
            && Math.Abs(actual - expectedValue) < 0.001) == true;
        if (hit)
        {
            return;
        }

        await Task.Delay(1000, cancellationToken);
    }

    throw new InvalidOperationException($"Timed out waiting for telemetry latest. Last API code={last?.Code}, msg={last?.Msg}");
}

static async Task PublishTelemetryByMqttAsync(
    string host,
    int port,
    string deviceName,
    string topicDeviceName,
    string productToken,
    string telemetryKey,
    double telemetryValue,
    bool useTls,
    CancellationToken cancellationToken)
{
    var factory = new MqttClientFactory();
    using var client = factory.CreateMqttClient();

    var builder = new MqttClientOptionsBuilder()
        .WithClientId("mqtt-sim-" + Guid.NewGuid().ToString("N"))
        .WithTcpServer(host, port)
        .WithCredentials(deviceName, productToken)
        .WithCleanSession();

    if (useTls)
    {
        builder.WithTlsOptions(new MqttClientTlsOptions
        {
            UseTls = true,
            IgnoreCertificateChainErrors = true,
            IgnoreCertificateRevocationErrors = true,
            AllowUntrustedCertificates = true
        });
    }

    var connect = await client.ConnectAsync(builder.Build(), cancellationToken);
    if (connect.ResultCode != MqttClientConnectResultCode.Success)
    {
        throw new InvalidOperationException($"MQTT connect failed: {connect.ResultCode} {connect.ReasonString}");
    }

    var payload = JsonSerializer.Serialize(
        new Dictionary<string, double> { [telemetryKey] = telemetryValue },
        AppJsonContext.Default.DictionaryStringDouble);

    var message = new MqttApplicationMessageBuilder()
        .WithTopic($"devices/{topicDeviceName}/telemetry")
        .WithPayload(Encoding.UTF8.GetBytes(payload))
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    var publish = await client.PublishAsync(message, cancellationToken);
    if (publish.ReasonCode != MqttClientPublishReasonCode.Success
        && publish.ReasonCode != MqttClientPublishReasonCode.NoMatchingSubscribers)
    {
        throw new InvalidOperationException($"MQTT publish failed: {publish.ReasonCode}");
    }

    await client.DisconnectAsync(cancellationToken: cancellationToken);
}

static async Task<ApiResult<T>> GetApiResultAsync<T>(HttpClient http, string path, CancellationToken cancellationToken)
{
    using var response = await http.GetAsync(path, cancellationToken);
    return await EnsureApiSuccessAsync<ApiResult<T>>(response, cancellationToken);
}

static async Task<T> EnsureApiSuccessAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
{
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }

    var parsed = JsonSerializer.Deserialize(body, AppJsonContext.JsonOptions.GetTypeInfo(typeof(T)));
    if (parsed is not T result)
    {
        throw new InvalidOperationException($"Unable to parse API response as {typeof(T).Name}: {body}");
    }

    if (result is ApiResultBase api && api.Code is not (0 or 10000))
    {
        throw new InvalidOperationException($"API failed. Code={api.Code}, Msg={api.Msg}, Body={body}");
    }

    return result;
}

static bool TryReadDouble(JsonElement value, out double result)
{
    if (value.ValueKind == JsonValueKind.Number)
    {
        return value.TryGetDouble(out result);
    }

    if (value.ValueKind == JsonValueKind.String)
    {
        return double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    result = 0;
    return false;
}

static void RequireBearer(SimulatorOptions options)
{
    if (string.IsNullOrWhiteSpace(options.BearerToken))
    {
        throw new InvalidOperationException("Creating a product requires --bearer. Or pass --product-token to use an existing product.");
    }
}

sealed record SimulatorOptions(
    Uri ApiBase,
    string MqttHost,
    int MqttPort,
    bool UseTls,
    string? BearerToken,
    string? ProductToken,
    string? ProductName,
    string? DeviceName,
    string? TopicDeviceName,
    string? TelemetryKey,
    double? TelemetryValue,
    TimeSpan Timeout,
    bool ShowHelp)
{
    public static SimulatorOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "-h" or "--help")
            {
                return Defaults() with { ShowHelp = true };
            }

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected argument: {arg}");
            }

            var key = arg[2..];
            if (key is "tls")
            {
                values[key] = "true";
                continue;
            }

            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for {arg}");
            }

            values[key] = args[++i];
        }

        var defaults = Defaults();
        return defaults with
        {
            ApiBase = new Uri(Get(values, "api-base") ?? defaults.ApiBase.ToString()),
            MqttHost = Get(values, "mqtt-host") ?? defaults.MqttHost,
            MqttPort = int.Parse(Get(values, "mqtt-port") ?? defaults.MqttPort.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            UseTls = values.ContainsKey("tls"),
            BearerToken = Get(values, "bearer"),
            ProductToken = Get(values, "product-token"),
            ProductName = Get(values, "product-name"),
            DeviceName = Get(values, "device-name"),
            TopicDeviceName = Get(values, "topic-device-name"),
            TelemetryKey = Get(values, "telemetry-key"),
            TelemetryValue = TryGetDouble(values, "telemetry-value"),
            Timeout = TimeSpan.FromSeconds(int.Parse(Get(values, "timeout-seconds") ?? "120", CultureInfo.InvariantCulture))
        };
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
        Usage:
          dotnet run --project tools/ProductMqttSimulator -- [options]

        Options:
          --api-base <url>          IoTSharp HTTP base URL. Default: http://localhost:2927
          --bearer <jwt>            Management API bearer token. Required when creating a product.
          --product-token <token>   Existing product token. If omitted, a product is created through HTTP.
          --product-name <name>     Product name. Default: generated.
          --device-name <name>      MQTT username and simulated device name. Default: generated.
          --topic-device-name <name> MQTT topic device segment. Default: me
          --mqtt-host <host>        MQTT host. Default: localhost
          --mqtt-port <port>        MQTT port. Default: 1883
          --tls                     Use MQTT TLS and accept the test certificate.
          --telemetry-key <key>     Telemetry key. Default: sim_temperature
          --telemetry-value <num>   Telemetry value. Default: generated.
          --timeout-seconds <sec>   Overall timeout. Default: 120
        """);
    }

    private static SimulatorOptions Defaults()
        => new(
            new Uri("http://localhost:2927"),
            "localhost",
            1883,
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            TimeSpan.FromSeconds(120),
            false);

    private static string? Get(Dictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value : null;

    private static double? TryGetDouble(Dictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}

abstract record ApiResultBase
{
    public int Code { get; init; }
    public string? Msg { get; init; }
}

sealed record ApiResult<T> : ApiResultBase
{
    public T? Data { get; init; }
}

sealed record ApiResultBool : ApiResultBase
{
    public bool Data { get; init; }
}

sealed record PagedData<T>
{
    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("rows")]
    public List<T> Rows { get; init; } = [];
}

sealed record ProductRow
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? ProductToken { get; init; }
    public List<DeviceRow> Devices { get; init; } = [];
}

sealed record DeviceRow
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
}

sealed record TelemetryDataDto
{
    public string? KeyName { get; init; }
    public JsonElement Value { get; init; }
}

[JsonSerializable(typeof(ApiResultBool))]
[JsonSerializable(typeof(ApiResult<ProductRow>))]
[JsonSerializable(typeof(ApiResult<PagedData<ProductRow>>))]
[JsonSerializable(typeof(ApiResult<List<TelemetryDataDto>>))]
[JsonSerializable(typeof(Dictionary<string, double>))]
sealed partial class AppJsonContext : JsonSerializerContext
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = Default
    };
}
