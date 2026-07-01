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

    private IFutureDockerImage? _sonnetDbImage;
    private IContainer? _sonnetDb;

    protected override async Task InitializeAppAsync()
    {
        var image = await CreateSonnetDbImageAsync();
        _sonnetDb = CreateSonnetDbContainer(image);

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

        if (_sonnetDbImage is not null)
        {
            await _sonnetDbImage.DisposeAsync();
        }
    }

    private async Task<IImage> CreateSonnetDbImageAsync()
    {
        var image = Environment.GetEnvironmentVariable("SONNETDB_TEST_IMAGE");
        if (!string.IsNullOrWhiteSpace(image))
        {
            return new DockerImage(image);
        }

        var sonnetDbRoot = GetSonnetDbRepositoryRoot();
        _sonnetDbImage = new ImageFromDockerfileBuilder()
            .WithContextDirectory(sonnetDbRoot)
            .WithDockerfileDirectory(Path.Combine(sonnetDbRoot, "src", "SonnetDB"))
            .WithDockerfile("Dockerfile")
            .WithName(new DockerImage("iotsharp/sonnetdb-test", Guid.NewGuid().ToString("N"), string.Empty))
            .WithImageBuildPolicy(PullPolicy.Always)
            .WithDeleteIfExists(true)
            .WithCleanUp(true)
            .Build();

        await _sonnetDbImage.CreateAsync(TestCancellationToken);

        return _sonnetDbImage;
    }

    private static IContainer CreateSonnetDbContainer(IImage image)
    {
        return new ContainerBuilder(image)
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

    private static string GetSonnetDbRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "SonnetDB");
            if (File.Exists(Path.Combine(candidate, "src", "SonnetDB", "Dockerfile")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Cannot find SonnetDB/src/SonnetDB/Dockerfile. Run tests from the IoTSharp repository checkout, or set SONNETDB_TEST_IMAGE.");
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
