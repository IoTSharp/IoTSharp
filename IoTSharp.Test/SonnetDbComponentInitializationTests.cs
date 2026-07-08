#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data.SonnetDB;
using IoTSharp.EventBus;
using IoTSharp.EventBus.SonnetMQ;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SonnetDB.Caching;
using SonnetDB.Data.Mq;
using SonnetDB.Data.ObjectStorage;
using Xunit;

namespace IoTSharp.Test;

public sealed class SonnetDbComponentInitializationTests : IDisposable
{
    private readonly string _root;

    public SonnetDbComponentInitializationTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "iotsharp-sonnetdb-components-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        TryDeleteDirectory(_root);
    }

    [Fact]
    public async Task SonnetDBStorage_WhenConstructed_PreparesTelemetryDatabase()
    {
        var telemetryPath = Path.Combine(_root, "telemetry");
        var settings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                ["TelemetryStorage"] = $"Data Source={telemetryPath};Measurement=TelemetryData;AutoCreate=true"
            }
        };

        var storage = new SonnetDBStorage(
            NullLogger<SonnetDBStorage>.Instance,
            Options.Create(settings));

        Assert.True(Directory.Exists(telemetryPath));
        Assert.True(await storage.CheckTelemetryStorage());
    }

    [Fact]
    public async Task UseSonnetMQ_WhenRegistered_PreparesEventDatabase()
    {
        var services = new ServiceCollection();
        var healthChecks = services.AddHealthChecks();
        var eventPath = Path.Combine(_root, "events");

        services.AddEventBus(options =>
        {
            options.AppSettings = new AppSettings
            {
                EventBus = EventBusFramework.SonnetMQ,
                EventBusMQ = EventBusMQ.SonnetMQ,
                ConsumerThreadCount = 1
            };
            options.EventBusStore = string.Empty;
            options.EventBusMQ = $"Data Source={eventPath}";
            options.HealthChecks = healthChecks;
            options.UseSonnetMQ();
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<SndbMqClient>();
        var stats = await client.GetStatsAsync("iotsharp.startup.probe");

        Assert.True(Directory.Exists(eventPath));
        Assert.Equal("iotsharp.startup.probe", stats.Topic);
    }

    [Fact]
    public async Task SonnetDbCacheStore_WhenRegistered_PreparesCacheDatabase()
    {
        var services = new ServiceCollection();
        var cachePath = Path.Combine(_root, "cache");

        services.AddSonnetDbEasyCaching("CachingUseIn-SonnetDB", options =>
        {
            options.ConnectionString = $"Data Source={cachePath}";
            options.Keyspace = "cache";
            options.Namespace = "iotsharp-test";
            options.ExpirationScanInterval = TimeSpan.Zero;
        });

        using var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<SonnetDbCacheStore>();
        var probe = await store.GetEntryAsync(SonnetDbCacheStore.StartupProbeKey);

        Assert.True(Directory.Exists(cachePath));
        Assert.NotNull(probe);
    }

    [Fact]
    public async Task SonnetDbBlobStorage_WhenConstructed_PreparesObjectDatabaseAndBucket()
    {
        var objectPath = Path.Combine(_root, "objects");
        var objectConnectionString = $"Data Source={objectPath}";
        var blobStorage = "sonnetdb://blob?bucket=iotsharp-blob-storage&connectionString="
                          + Uri.EscapeDataString(objectConnectionString);

        var parsed = SonnetDbBlobStorage.ParseConnectionString(blobStorage);
        using var storage = new SonnetDbBlobStorage(parsed.ConnectionString, parsed.Bucket);
        using var client = new SndbObjectStorageClient(objectConnectionString);
        var buckets = await client.ListBucketsAsync();

        Assert.True(Directory.Exists(objectPath));
        Assert.Contains(buckets, bucket => bucket.Name == "iotsharp-blob-storage");
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // 测试清理失败不影响真实断言结果。
        }
    }
}
