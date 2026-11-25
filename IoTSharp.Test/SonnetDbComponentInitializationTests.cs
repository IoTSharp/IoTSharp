#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using IoTSharp.EventBus;
using IoTSharp.EventBus.SonnetMQ;
using IoTSharp.Extensions;
using IoTSharp.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SonnetDB.Data;
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

    [Fact]
    public async Task SonnetMQ_WriteRecoveryRequired_RetriesAtSameOffsetWithoutDeadLetter()
    {
        var storage = new GuardedTelemetryStorage(individualDispatch: false);
        await VerifyGuardedTelemetryAsync(storage, expectRecovery: true);
    }

    [Fact]
    public async Task SonnetMQ_WriteRecoveryDuringBatchFallback_HaltsWithoutRetryOrDeadLetter()
    {
        var storage = new GuardedTelemetryStorage(individualDispatch: true);
        await VerifyGuardedTelemetryAsync(storage, expectRecovery: false);
    }

    [Fact]
    public async Task SonnetMQ_PartialWriteRecovery_HaltsWithoutRetryOrDeadLetter()
    {
        var storage = new GuardedTelemetryStorage(individualDispatch: false, partial: true);
        await VerifyGuardedTelemetryAsync(storage, expectRecovery: false);
    }

    [Fact]
    public async Task SonnetMQ_WriteRecoveryFromRules_HaltsAfterSuccessfulStorage()
    {
        var storage = new GuardedTelemetryStorage(individualDispatch: false);
        await VerifyGuardedTelemetryAsync(storage, expectRecovery: false, guardFromRules: true);
    }

    private async Task VerifyGuardedTelemetryAsync(
        GuardedTelemetryStorage storage, bool expectRecovery, bool guardFromRules = false)
    {
        const string topic = "iotsharp.services.datastream.telemetrydata";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCaching(options => options.UseInMemory("CachingUseIn-InMemory"));
        services.AddSingleton<IStorage>(storage);
        var healthChecks = services.AddHealthChecks();
        services.AddEventBus(options =>
        {
            options.AppSettings = new AppSettings
            {
                EventBus = EventBusFramework.SonnetMQ,
                EventBusMQ = EventBusMQ.SonnetMQ,
                ConsumerThreadCount = 1
            };
            options.EventBusMQ = $"Data Source={Path.Combine(_root, "guarded-events")}";
            options.HealthChecks = healthChecks;
            if (guardFromRules)
            {
                storage.AllowWrites();
                options.RunRules = (_, _, _) => storage.Reject();
            }
            options.UseSonnetMQ();
        });

        await using var provider = services.BuildServiceProvider();
        var worker = Assert.Single(provider.GetServices<IHostedService>().OfType<SonnetMqEventBusWorker>());
        var client = provider.GetRequiredService<SndbMqClient>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
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
            await storage.FirstGuard.Task.WaitAsync(timeout.Token);
            if (expectRecovery)
            {
                // 超过原三次死信阈值，确认恢复等待独立于数据错误的重试计数。
                await storage.ThirdGuard.Task.WaitAsync(timeout.Token);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1250), timeout.Token);
                Assert.Equal(1, storage.GuardAttempts);
                Assert.Equal(guardFromRules ? 1 : 0, storage.SuccessfulWrites);
            }

            Assert.Equal(0, (await client.GetStatsAsync(topic, timeout.Token)).ConsumerOffsets.GetValueOrDefault("iotsharp"));
            Assert.Equal(0, (await client.GetStatsAsync(topic + ".errors", timeout.Token)).NextOffset);
            Assert.Equal(storage.IndividualDispatch ? storage.GuardAttempts : 0, storage.IndividualWrites);

            if (!expectRecovery)
                return;

            storage.AllowWrites();
            for (var attempt = 0; attempt < 80; attempt++)
            {
                var stats = await client.GetStatsAsync(topic, timeout.Token);
                if (stats.ConsumerOffsets.GetValueOrDefault("iotsharp") == 1)
                {
                    Assert.Equal(1, storage.SuccessfulWrites);
                    Assert.Equal(0, (await client.GetStatsAsync(topic + ".errors", timeout.Token)).NextOffset);
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
            }
            Assert.Fail("Telemetry failed to resume from its held position.");
        }
        finally
        {
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await worker.StopAsync(shutdown.Token);
        }
    }

    private sealed class GuardedTelemetryStorage(bool individualDispatch, bool partial = false)
        : EFStorage(NullLogger<EFStorage>.Instance, null!, Options.Create(new AppSettings()))
    {
        private int _guards;
        private int _individualWrites;
        private int _successfulWrites;
        private int _allowWrites;

        internal TaskCompletionSource FirstGuard { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource ThirdGuard { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool IndividualDispatch => individualDispatch;
        internal int GuardAttempts => Volatile.Read(ref _guards);
        internal int IndividualWrites => Volatile.Read(ref _individualWrites);
        internal int SuccessfulWrites => Volatile.Read(ref _successfulWrites);

        internal void AllowWrites() => Volatile.Write(ref _allowWrites, 1);

        public override Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            Interlocked.Increment(ref _individualWrites);
            Store();
            return Task.FromResult<(bool, List<TelemetryData>)>((true, []));
        }

        public override Task<TelemetryBatchStoreResult> StoreTelemetryBatchAsync(IReadOnlyCollection<PlayloadData> messages)
        {
            if (individualDispatch)
                return Task.FromResult(new TelemetryBatchStoreResult(false, [], messages.Count));

            Store();
            return Task.FromResult(new TelemetryBatchStoreResult(true, [], messages.Count));
        }

        internal Task Reject()
        {
            int attempts = Interlocked.Increment(ref _guards);
            FirstGuard.TrySetResult();
            if (attempts == 3)
                ThirdGuard.TrySetResult();

            throw new SndbServerException(partial ? SonnetDbWriteRecovery.Partial : SonnetDbWriteRecovery.Required,
                "Synthetic write recovery guard.", partial ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable);
        }

        private void Store()
        {
            if (Volatile.Read(ref _allowWrites) != 0)
            {
                Interlocked.Increment(ref _successfulWrites);
                return;
            }

            Reject();
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SonnetMQ_CompletedTelemetryBatch_UsesOneCumulativeAcknowledgement(bool failFirstAck)
    {
        const string topic = "iotsharp.services.datastream.telemetrydata";
        var timestamp = DateTimeOffset.UtcNow;
        var payload = Encoding.UTF8.GetBytes(JsonObjectSerializer.Serialize(new PlayloadData
        {
            DeviceId = Guid.NewGuid(),
            DataCatalog = DataCatalog.TelemetryData,
            DataSide = DataSide.ClientSide,
            MsgBody = new Dictionary<string, object> { ["temperature"] = 22.5 }
        }));
        var messages = new long[] { 10, 11, 12 }.Select(offset => new
        {
            topic,
            offset,
            timestampUtc = timestamp,
            headers = new Dictionary<string, string>(),
            payload
        }).ToArray();
        var storage = new RecordingTelemetryStorage();
        var ruleCalls = 0;
        var acknowledgements = new ConcurrentQueue<(long Offset, int BatchWrites, int RuleCalls)>();
        var acknowledgementCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var broker = builder.Build();
        broker.MapPost("/v1/db/events/mq/{topic}/pull", (string topic) => Results.Json(new
        {
            messages = topic == messages[0].topic && !acknowledgementCompleted.Task.IsCompleted ? messages : []
        }));
        broker.MapPost($"/v1/db/events/mq/{topic}/ack", async (HttpContext context) =>
        {
            var request = await context.Request.ReadFromJsonAsync<JsonElement>(context.RequestAborted);
            var offset = request.GetProperty("offset").GetInt64();
            acknowledgements.Enqueue((offset, storage.BatchWrites, Volatile.Read(ref ruleCalls)));
            if (failFirstAck && acknowledgements.Count == 1)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            if (offset == messages[^1].offset)
            {
                acknowledgementCompleted.TrySetResult();
            }
            return Results.Json(new { topic, consumerGroup = "iotsharp", nextOffset = offset + 1 });
        });

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        await broker.StartAsync(timeout.Token);
        using var client = new SndbMqClient($"Data Source=sonnetdb+{broker.Urls.Single()}/events;Protocol=Rest;Timeout=2");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCaching(options => options.UseInMemory("CachingUseIn-InMemory"));
        services.AddSingleton<IStorage>(storage);
        var healthChecks = services.AddHealthChecks();
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
        services.AddSingleton(client);

        await using var provider = services.BuildServiceProvider();
        var worker = Assert.Single(provider.GetServices<IHostedService>().OfType<SonnetMqEventBusWorker>());
        await worker.StartAsync(timeout.Token);
        try
        {
            await acknowledgementCompleted.Task.WaitAsync(timeout.Token);
            Assert.Equal(0, storage.IndividualWrites);
            Assert.Equal(failFirstAck ? [(12L, 1, 6), (12L, 2, 12)] : new[] { (12L, 1, 6) }, acknowledgements.ToArray());
        }
        finally
        {
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await worker.StopAsync(shutdown.Token);
            await broker.StopAsync(shutdown.Token);
        }
    }

    private sealed class RecordingTelemetryStorage()
        : EFStorage(NullLogger<EFStorage>.Instance, null!, Options.Create(new AppSettings()))
    {
        private int _batchWrites;
        private int _individualWrites;

        public int BatchWrites => Volatile.Read(ref _batchWrites);
        public int IndividualWrites => Volatile.Read(ref _individualWrites);

        public override Task<(bool result, List<TelemetryData> telemetries)> StoreTelemetryAsync(PlayloadData msg)
        {
            Interlocked.Increment(ref _individualWrites);
            return Task.FromResult<(bool, List<TelemetryData>)>((true, []));
        }

        public override Task<TelemetryBatchStoreResult> StoreTelemetryBatchAsync(IReadOnlyCollection<PlayloadData> messages)
        {
            Interlocked.Increment(ref _batchWrites);
            return Task.FromResult(new TelemetryBatchStoreResult(true, [], messages.Count));
        }
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
