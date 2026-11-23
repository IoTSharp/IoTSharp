#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using IoTSharp.EventBus;
using IoTSharp.EventBus.SonnetMQ;
using IoTSharp.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
    public async Task SonnetMQ_TelemetryStorageFailure_RetriesBeforeAcknowledgingDeadLetter()
    {
        const string topic = "iotsharp.services.datastream.telemetrydata";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCaching(options => options.UseInMemory("CachingUseIn-InMemory"));
        var storage = new FailingTelemetryStorage();
        services.AddSingleton<IStorage>(storage);
        var healthChecks = services.AddHealthChecks();
        var ruleCalls = 0;
        services.AddEventBus(options =>
        {
            options.AppSettings = new AppSettings
            {
                EventBus = EventBusFramework.SonnetMQ,
                EventBusMQ = EventBusMQ.SonnetMQ,
                ConsumerThreadCount = 1
            };
            options.EventBusMQ = $"Data Source={Path.Combine(_root, "events")}";
            options.HealthChecks = healthChecks;
            options.RunRules = (_, _, _) =>
            {
                Interlocked.Increment(ref ruleCalls);
                return Task.CompletedTask;
            };
            options.UseSonnetMQ();
        });

        await using var provider = services.BuildServiceProvider();
        var worker = Assert.Single(provider.GetServices<IHostedService>().OfType<SonnetMqEventBusWorker>());
        var client = provider.GetRequiredService<SndbMqClient>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await worker.StartAsync(timeout.Token);
        try
        {
            await provider.GetRequiredService<IPublisher>().PublishTelemetryData(new PlayloadData
            {
                DeviceId = Guid.NewGuid(),
                DataCatalog = DataCatalog.TelemetryData,
                DataSide = DataSide.ClientSide,
                MsgBody = new Dictionary<string, object> { ["temperature"] = 22.5 }
            });

            for (var attempt = 0; attempt < 80; attempt++)
            {
                var pending = await client.GetStatsAsync(topic, timeout.Token);
                var deadLetters = await client.GetStatsAsync(topic + ".errors", timeout.Token);
                if (deadLetters.NextOffset == 1 && pending.ConsumerOffsets.GetValueOrDefault("iotsharp") == 1)
                {
                    Assert.Equal(3, storage.Attempts);
                    Assert.Equal(0, Volatile.Read(ref ruleCalls));
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
            }

            Assert.Fail("Telemetry was not retried and moved to the dead-letter topic.");
        }
        finally
        {
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await worker.StopAsync(shutdown.Token);
        }
    }

    private sealed class FailingTelemetryStorage()
        : EFStorage(NullLogger<EFStorage>.Instance, null!, Options.Create(new AppSettings()))
    {
        private int _attempts;

        public int Attempts => Volatile.Read(ref _attempts);

        public override Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            Interlocked.Increment(ref _attempts);
            return Task.FromResult<(bool, List<TelemetryData>)>((false, []));
        }

        public override Task<TelemetryBatchStoreResult> StoreTelemetryBatchAsync(IReadOnlyCollection<PlayloadData> messages)
            => Task.FromResult(new TelemetryBatchStoreResult(false, [], messages.Count));
    }

    [Fact]
    public void SonnetDbEasyCaching_WhenRegisteredThroughEasyCaching_PreparesCacheDatabase()
    {
        var services = new ServiceCollection();
        var cachePath = Path.Combine(_root, "cache");

        services.AddEasyCaching(options =>
        {
            options.UseSonnetDB(config =>
            {
                config.ConnectionString = $"Data Source={cachePath}";
                config.Keyspace = "cache";
                config.Namespace = "iotsharp-test";
                config.ExpirationScanInterval = TimeSpan.Zero;
            }, "CachingUseIn-SonnetDB");
        });

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IEasyCachingProviderFactory>();
        var cache = factory.GetCachingProvider("CachingUseIn-SonnetDB");
        cache.Set("startup-probe", "ok", TimeSpan.FromMinutes(1));

        Assert.True(Directory.Exists(cachePath));
        Assert.Equal("ok", cache.Get<string>("startup-probe").Value);
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
