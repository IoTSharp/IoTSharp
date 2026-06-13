using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SonnetDB.Data;
using Xunit;

namespace IoTSharp.Test;

public sealed class SonnetDBStorageTests : IDisposable
{
    private readonly string _root;

    public SonnetDBStorageTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "iotsharp-sonnetdb-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch
        {
        }
    }

    [Fact]
    public async Task StoreTelemetryAsync_WhenUsingSonnetDB_CreatesOneMeasurementPerDevice()
    {
        var deviceA = Guid.NewGuid();
        var deviceB = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");

        var resultA = await storage.StoreTelemetryAsync(CreatePayload(deviceA, 22.5));
        var resultB = await storage.StoreTelemetryAsync(CreatePayload(deviceB, 31.75));

        Assert.True(resultA.result);
        Assert.True(resultB.result);
        Assert.True(MeasurementHasRows($"TelemetryData_{deviceA:N}"));
        Assert.True(MeasurementHasRows($"TelemetryData_{deviceB:N}"));

        var latestA = await storage.GetTelemetryLatest(deviceA, "temperature");
        var latestB = await storage.GetTelemetryLatest(deviceB, "temperature");

        Assert.Equal(22.5, Assert.Single(latestA).Value);
        Assert.Equal(31.75, Assert.Single(latestB).Value);
    }

    [Fact]
    public async Task StoreTelemetryAsync_WhenPayloadAddsNewKeys_EvolvesMeasurementSchema()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");

        var first = await storage.StoreTelemetryAsync(CreatePayload(deviceId, new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc), new Dictionary<string, object>
        {
            ["temperature"] = 22.5
        }));
        var second = await storage.StoreTelemetryAsync(CreatePayload(deviceId, new DateTime(2026, 6, 12, 1, 1, 0, DateTimeKind.Utc), new Dictionary<string, object>
        {
            ["temperature"] = 23.5,
            ["humidity"] = 61L
        }));

        Assert.True(first.result);
        Assert.True(second.result);

        var latest = await storage.GetTelemetryLatest(deviceId);
        Assert.Contains(latest, x => x.KeyName == "temperature" && Equals(23.5, x.Value));
        Assert.Contains(latest, x => x.KeyName == "humidity" && Equals(61L, x.Value));
    }

    [Fact]
    public async Task LoadTelemetryAsync_WhenUsingSonnetDB_ReturnsHistoryAndAggregates()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");
        var begin = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);

        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddMinutes(0), new Dictionary<string, object> { ["temperature"] = 20.0 }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddMinutes(1), new Dictionary<string, object> { ["temperature"] = 22.0 }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddMinutes(3), new Dictionary<string, object> { ["temperature"] = 30.0 }));

        var history = await storage.LoadTelemetryAsync(deviceId, "temperature", begin, begin.AddMinutes(4), TimeSpan.Zero, Aggregate.None);
        var aggregate = await storage.LoadTelemetryAsync(deviceId, "temperature", begin, begin.AddMinutes(4), TimeSpan.FromMinutes(2), Aggregate.Mean);

        Assert.Equal(new[] { 20.0, 22.0, 30.0 }, history.Select(x => (double)x.Value).OrderBy(x => x).ToArray());
        Assert.Contains(aggregate, x => x.KeyName == "temperature" && Equals(21.0, x.Value));
        Assert.Contains(aggregate, x => x.KeyName == "temperature" && Equals(30.0, x.Value));
    }

    [Fact]
    public async Task LoadTelemetryAsync_WhenUsingMedian_DelegatesToSonnetDbMedian()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");
        var begin = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);

        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddSeconds(0), new Dictionary<string, object> { ["temperature"] = 10.0 }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddSeconds(10), new Dictionary<string, object> { ["temperature"] = 20.0 }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddSeconds(20), new Dictionary<string, object> { ["temperature"] = 30.0 }));

        var aggregate = await storage.LoadTelemetryAsync(deviceId, "temperature", begin, begin.AddMinutes(1), TimeSpan.FromMinutes(1), Aggregate.Median);
        var point = Assert.Single(aggregate);

        Assert.Equal("temperature", point.KeyName);
        Assert.Equal(DataType.Double, point.DataType);
        Assert.InRange((double)point.Value, 18.0, 22.0);
    }

    [Fact]
    public async Task StoreTelemetryBatchAsync_WhenUsingSonnetDB_WritesRowsInBulk()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");
        var begin = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);
        var messages = Enumerable.Range(0, 5)
            .Select(i => CreatePayload(deviceId, begin.AddMinutes(i), new Dictionary<string, object>
            {
                ["temperature"] = 20.0 + i,
                ["humidity"] = 60L + i
            }))
            .ToArray();

        var report = await storage.StoreTelemetryBatchAsync(messages);
        var history = await storage.LoadTelemetryAsync(deviceId, "temperature", begin, begin.AddMinutes(5), TimeSpan.Zero, Aggregate.None);
        var latest = await storage.GetTelemetryLatest(deviceId, "temperature,humidity");

        Assert.True(report.IsComplete);
        Assert.Equal(5, report.MessageCount);
        Assert.Equal(1, report.BatchCount);
        Assert.Equal(5, report.WrittenRows);
        Assert.Equal(10, report.TelemetryValueCount);
        Assert.Equal(0, report.SkippedTelemetryValueCount);
        Assert.Equal(5, history.Count);
        Assert.Contains(latest, x => x.KeyName == "temperature" && Equals(24.0, x.Value));
        Assert.Contains(latest, x => x.KeyName == "humidity" && Equals(64L, x.Value));
    }

    [Fact]
    public async Task StoreTelemetryAsync_WhenSchemaCacheLimitIsExceeded_ReloadsEvictedMeasurementSchema()
    {
        var deviceA = Guid.NewGuid();
        var deviceB = Guid.NewGuid();
        var deviceC = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData", _root, schemaCacheLimit: 2);
        var begin = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);

        await storage.StoreTelemetryAsync(CreatePayload(deviceA, begin, new Dictionary<string, object> { ["temperature"] = 20.0 }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceB, begin, new Dictionary<string, object> { ["humidity"] = 61L }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceC, begin, new Dictionary<string, object> { ["voltage"] = 12.5 }));
        var result = await storage.StoreTelemetryAsync(CreatePayload(deviceA, begin.AddMinutes(1), new Dictionary<string, object> { ["temperature"] = 21.0 }));

        var latest = await storage.GetTelemetryLatest(deviceA, "temperature");

        Assert.True(result.result);
        Assert.Equal(21.0, Assert.Single(latest).Value);
    }

    [Fact]
    public async Task GetTelemetryLatest_WhenFieldsAreSparse_ReturnsLatestValuePerField()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");
        var begin = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);

        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin, new Dictionary<string, object>
        {
            ["temperature"] = 20.0,
            ["humidity"] = 61L
        }));
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, begin.AddMinutes(1), new Dictionary<string, object>
        {
            ["temperature"] = 21.0
        }));

        var latest = await storage.GetTelemetryLatest(deviceId, "temperature,humidity");

        Assert.Contains(latest, x => x.KeyName == "temperature" && Equals(21.0, x.Value));
        Assert.Contains(latest, x => x.KeyName == "humidity" && Equals(61L, x.Value));
    }

    [Fact]
    public async Task CheckTelemetryStorage_WhenUsingSonnetDB_RunsQueryProbe()
    {
        var storage = CreateStorage("TelemetryData");

        Assert.True(await storage.CheckTelemetryStorage());
    }

    [Fact]
    public async Task BackupRestoreAndReplay_WhenUsingEmbeddedSonnetDB_PreserveTelemetry()
    {
        var deviceId = Guid.NewGuid();
        var storage = CreateStorage("TelemetryData");
        var timestamp = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc);
        await storage.StoreTelemetryAsync(CreatePayload(deviceId, timestamp, new Dictionary<string, object>
        {
            ["temperature"] = 22.5,
            ["humidity"] = 61L
        }));

        var backupDirectory = _root + "-backup";
        var restoreDirectory = _root + "-restored";
        var backup = storage.CreateBackup(backupDirectory);
        var verification = storage.VerifyBackup(backupDirectory);
        var dryRun = storage.RestoreDryRun(backupDirectory, restoreDirectory);
        var restore = storage.Restore(backupDirectory, restoreDirectory);
        var replay = await storage.ReplayFailureAsync(deviceId, "temperature,humidity", timestamp.AddMinutes(-1), timestamp.AddMinutes(1));

        Assert.True(backup.FileCount > 0);
        Assert.True(backup.MeasurementCount >= 1);
        Assert.True(verification.IsValid);
        Assert.True(dryRun.IsValid);
        Assert.True(restore.FileCount > 0);
        Assert.Equal(2, replay.PointCount);
        Assert.Contains("temperature", replay.Fields);
        Assert.Contains("humidity", replay.Fields);

        var restoredStorage = CreateStorage("TelemetryData", restoreDirectory);
        var latest = await restoredStorage.GetTelemetryLatest(deviceId);
        Assert.Contains(latest, x => x.KeyName == "temperature" && Equals(22.5, x.Value));
        Assert.Contains(latest, x => x.KeyName == "humidity" && Equals(61L, x.Value));
    }

    private SonnetDBStorage CreateStorage(string measurementPrefix)
        => CreateStorage(measurementPrefix, _root);

    private static SonnetDBStorage CreateStorage(string measurementPrefix, string root, int? schemaCacheLimit = null)
    {
        var telemetryStorage = $"Data Source={root};Measurement={measurementPrefix};AutoCreate=true";
        if (schemaCacheLimit.HasValue)
        {
            telemetryStorage += $";SchemaCacheLimit={schemaCacheLimit.Value}";
        }

        var settings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                ["TelemetryStorage"] = telemetryStorage
            }
        };

        return new SonnetDBStorage(NullLogger<SonnetDBStorage>.Instance, Options.Create(settings));
    }

    private static PlayloadData CreatePayload(Guid deviceId, double temperature)
    {
        return CreatePayload(deviceId, new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc), new Dictionary<string, object>
        {
            ["temperature"] = temperature
        });
    }

    private static PlayloadData CreatePayload(Guid deviceId, DateTime timestamp, Dictionary<string, object> values)
    {
        return new PlayloadData
        {
            DeviceId = deviceId,
            ts = timestamp,
            DataSide = DataSide.ClientSide,
            DataCatalog = DataCatalog.TelemetryData,
            MsgBody = values
        };
    }

    private bool MeasurementHasRows(string measurement)
    {
        using var connection = new SndbConnection($"Data Source={_root}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT time, temperature FROM \"{measurement}\"";
        try
        {
            using var reader = command.ExecuteReader();
            return reader.Read();
        }
        catch (Exception)
        {
            return false;
        }
    }
}
