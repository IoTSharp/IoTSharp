#nullable enable

using System;
using System.Collections.Generic;
using IoTSharp.Contracts;

namespace IoTSharp.Test;

public sealed class IoTSharpTestProfile
{
    public required string Name { get; init; }

    public required DataBaseType DataBase { get; init; }

    public required TelemetryStorage TelemetryStorage { get; init; }

    public EventBusFramework EventBus { get; init; } = EventBusFramework.CAP;

    public EventBusStore EventBusStore { get; init; } = EventBusStore.InMemory;

    public EventBusMQ EventBusMQ { get; init; } = EventBusMQ.InMemory;

    public CachingUseIn CachingUseIn { get; init; } = CachingUseIn.InMemory;

    public string? MainConnectionString { get; init; }

    public string? TelemetryConnectionString { get; init; }

    public string? EventBusStoreConnectionString { get; init; }

    public string? EventBusMqConnectionString { get; init; }

    public string? BlobStorageConnectionString { get; init; }

    public string? CachingUseRedisHosts { get; init; }

    public string? CachingUseSonnetDBConnectionString { get; init; }

    public string CachingUseSonnetDBKeyspace { get; init; } = "cache";

    public string CachingUseSonnetDBNamespace { get; init; } = "iotsharp-test";

    public int ConsumerThreadCount { get; init; } = 2;

    public int DbContextPoolSize { get; init; } = 32;

    public bool SupportsExternalEventBus => EventBusStore != EventBusStore.InMemory || EventBusMQ != EventBusMQ.InMemory;

    public IReadOnlyDictionary<string, string> ToHostSettings()
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DataBase"] = Enum.GetName(DataBase)!,
            ["TelemetryStorage"] = Enum.GetName(TelemetryStorage)!,
            ["EventBus"] = Enum.GetName(EventBus)!,
            ["EventBusStore"] = Enum.GetName(EventBusStore)!,
            ["EventBusMQ"] = Enum.GetName(EventBusMQ)!,
            ["CachingUseIn"] = Enum.GetName(CachingUseIn)!,
            ["ConsumerThreadCount"] = ConsumerThreadCount.ToString(),
            ["DbContextPoolSize"] = DbContextPoolSize.ToString(),
            ["JwtKey"] = "iotsharp-test-jwt-key-change-me-32-bytes-minimum",
            ["JwtExpireHours"] = "24",
            ["JwtIssuer"] = "IoTSharp.Net",
            ["JwtAudience"] = "IoTSharp.Net",
            ["ConnectionStrings:IoTSharp"] = Required(MainConnectionString, nameof(MainConnectionString)),
            ["ConnectionStrings:TelemetryStorage"] = Required(TelemetryConnectionString, nameof(TelemetryConnectionString)),
            ["ConnectionStrings:EventBusStore"] = EventBusStoreConnectionString ?? string.Empty,
            ["ConnectionStrings:EventBusMQ"] = EventBusMqConnectionString ?? string.Empty,
            ["ConnectionStrings:BlobStorage"] = BlobStorageConnectionString ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(CachingUseRedisHosts))
        {
            settings["CachingUseRedisHosts"] = CachingUseRedisHosts!;
        }

        if (!string.IsNullOrWhiteSpace(CachingUseSonnetDBConnectionString))
        {
            settings["CachingUseSonnetDBConnectionString"] = CachingUseSonnetDBConnectionString!;
            settings["CachingUseSonnetDBKeyspace"] = CachingUseSonnetDBKeyspace;
            settings["CachingUseSonnetDBNamespace"] = CachingUseSonnetDBNamespace;
        }

        return settings;
    }

    private static string Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required for an IoTSharp test profile.");
        }

        return value;
    }
}
