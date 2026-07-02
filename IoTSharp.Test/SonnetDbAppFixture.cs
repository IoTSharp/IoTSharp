#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using IoTSharp.Contracts;
using Testcontainers.SonnetDB;

namespace IoTSharp.Test;

public sealed class SonnetDbAppFixture : AppInstance
{
    private const string AdminToken = "iotsharp_sonnetdb_test_token_change_me";

    private IFutureDockerImage? _sonnetDbImage;
    private SonnetDbContainer? _sonnetDb;

    protected override async Task InitializeAppAsync()
    {
        var image = await CreateSonnetDbImageAsync();
        _sonnetDb = CreateSonnetDbContainer(image);

        await _sonnetDb.StartAsync(TestCancellationToken);
        await EnsureDatabasesAsync();

        var main = _sonnetDb.GetConnectionString("iotsharp");
        var telemetry = $"{_sonnetDb.GetConnectionString("telemetry")};Measurement=TelemetryData;AutoCreate=true";
        var events = _sonnetDb.GetConnectionString("events");
        var cache = _sonnetDb.GetConnectionString("cache");
        var objects = _sonnetDb.GetConnectionString("objects");

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

    private static SonnetDbContainer CreateSonnetDbContainer(IImage image)
    {
        return new SonnetDbBuilder(image)
            .WithAdminToken(AdminToken)
            .WithDatabase("iotsharp")
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

    private async Task EnsureDatabasesAsync()
    {
        foreach (var database in new[] { "telemetry", "events", "cache", "objects" })
        {
            await _sonnetDb!.CreateDatabaseAsync(database, TestCancellationToken);
        }
    }
}
