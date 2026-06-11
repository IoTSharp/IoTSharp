using System;
using System.Collections.Generic;
using System.IO;
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

    private SonnetDBStorage CreateStorage(string measurementPrefix)
    {
        var settings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                ["TelemetryStorage"] = $"Data Source={_root};Measurement={measurementPrefix};AutoCreate=true"
            }
        };

        return new SonnetDBStorage(NullLogger<SonnetDBStorage>.Instance, Options.Create(settings));
    }

    private static PlayloadData CreatePayload(Guid deviceId, double temperature)
    {
        return new PlayloadData
        {
            DeviceId = deviceId,
            ts = new DateTime(2026, 6, 12, 1, 0, 0, DateTimeKind.Utc),
            DataSide = DataSide.ClientSide,
            DataCatalog = DataCatalog.TelemetryData,
            MsgBody = new Dictionary<string, object>
            {
                ["temperature"] = temperature
            }
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
