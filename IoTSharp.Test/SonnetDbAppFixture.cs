#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using IoTSharp.Contracts;

namespace IoTSharp.Test;

public sealed class SonnetDbAppFixture : AppInstance
{
    private const ushort SonnetDbPort = 5080;
    private const string AdminToken = "iotsharp_sonnetdb_test_token_change_me";

    private IContainer? _sonnetDb;

    protected override async Task InitializeAppAsync()
    {
        _sonnetDb = CreateSonnetDbContainer();

        await _sonnetDb.StartAsync(TestCancellationToken);

        var baseUrl = $"http://{_sonnetDb.Hostname}:{_sonnetDb.GetMappedPublicPort(SonnetDbPort)}";
        await EnsureDatabasesAsync(baseUrl);

        var main = BuildConnectionString(baseUrl, "iotsharp");
        var telemetry = $"{BuildConnectionString(baseUrl, "telemetry")};Measurement=TelemetryData;AutoCreate=true";
        var events = BuildConnectionString(baseUrl, "events");
        var cache = BuildConnectionString(baseUrl, "cache");
        var objects = BuildConnectionString(baseUrl, "objects");

        await InitializeApplicationAsync(new IoTSharpTestProfile
        {
            Name = "sonnetdb-container",
            DataBase = DataBaseType.SonnetDB,
            TelemetryStorage = TelemetryStorage.SonnetDB,
            EventBus = EventBusFramework.SonnetMQ,
            EventBusStore = EventBusStore.InMemory,
            EventBusMQ = EventBusMQ.SonnetMQ,
            CachingUseIn = CachingUseIn.SonnetDB,
            MainConnectionString = main,
            TelemetryConnectionString = telemetry,
            EventBusMqConnectionString = events,
            CachingUseSonnetDBConnectionString = cache,
            BlobStorageConnectionString = "sonnetdb://blob?bucket=iotsharp-blob-storage&connectionString=" + Uri.EscapeDataString(objects)
        });
    }

    protected override async Task DisposeTestResourcesAsync()
    {
        if (_sonnetDb is not null)
        {
            await _sonnetDb.DisposeAsync();
        }
    }

    private static IContainer CreateSonnetDbContainer()
    {
        var image = Environment.GetEnvironmentVariable("SONNETDB_TEST_IMAGE");
        var builder = string.IsNullOrWhiteSpace(image)
            ? new ContainerBuilder("mcr.microsoft.com/dotnet/aspnet:10.0")
                .WithBindMount(GetLocalSonnetDbOutputDirectory(), "/app", AccessMode.ReadOnly)
                .WithWorkingDirectory("/app")
                .WithCommand("dotnet", "SonnetDB.dll")
            : new ContainerBuilder(image);

        return builder
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithPortBinding(SonnetDbPort, true)
            .WithEnvironment("TZ", "Asia/Shanghai")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:5080")
            .WithEnvironment("SONNETDB_SonnetDBServer__DataRoot", "/data")
            .WithEnvironment("SONNETDB_SonnetDBServer__AutoLoadExistingDatabases", "true")
            .WithEnvironment("SONNETDB_SonnetDBServer__AllowAnonymousProbes", "true")
            .WithEnvironment($"SONNETDB_SonnetDBServer__Tokens__{AdminToken}", "admin")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
                .ForPort(SonnetDbPort)
                .ForPath("/healthz")))
            .Build();
    }

    private static string GetLocalSonnetDbOutputDirectory()
    {
        var testOutput = AppContext.BaseDirectory;
        if (File.Exists(Path.Combine(testOutput, "SonnetDB.dll")))
        {
            return testOutput;
        }

        for (var directory = new DirectoryInfo(testOutput); directory is not null; directory = directory.Parent)
        {
            foreach (var configuration in new[] { "Debug", "Release" })
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "SonnetDB",
                    "src",
                    "SonnetDB",
                    "bin",
                    configuration,
                    "net10.0");
                if (File.Exists(Path.Combine(candidate, "SonnetDB.dll")))
                {
                    return candidate;
                }
            }
        }

        throw new DirectoryNotFoundException(
            "Cannot find local SonnetDB server build output. Build SonnetDB/src/SonnetDB/SonnetDB.csproj first, or set SONNETDB_TEST_IMAGE.");
    }

    private static string BuildConnectionString(string baseUrl, string database)
        => $"Data Source=sonnetdb+{baseUrl}/{database};Token={AdminToken};Timeout=30";

    private async Task EnsureDatabasesAsync(string baseUrl)
    {
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        foreach (var database in new[] { "telemetry", "events", "cache", "objects" })
        {
            using var response = await client.PostAsJsonAsync("/v1/db", new { name = database }, TestCancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(TestCancellationToken);
                throw new InvalidOperationException($"Create SonnetDB database '{database}' failed: {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
            }
        }
    }
}
