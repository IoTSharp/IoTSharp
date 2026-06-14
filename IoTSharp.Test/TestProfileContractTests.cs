#nullable enable

using System;
using System.Linq;
using IoTSharp.Contracts;
using Xunit;

namespace IoTSharp.Test;

public sealed class TestProfileContractTests
{
    [Fact]
    public void PostgreSqlInfluxRabbitMongoProfile_DescribesPlannedExternalStack()
    {
        var profile = new IoTSharpTestProfile
        {
            Name = "postgresql-influxdb-rabbitmq-mongodb",
            DataBase = DataBaseType.PostgreSql,
            TelemetryStorage = TelemetryStorage.InfluxDB,
            EventBus = EventBusFramework.CAP,
            EventBusStore = EventBusStore.MongoDB,
            EventBusMQ = EventBusMQ.RabbitMQ,
            MainConnectionString = "Host=postgres;Database=iotsharp",
            TelemetryConnectionString = "http://influxdb:8086/?org=iotsharp&bucket=iotsharp&token=test",
            EventBusStoreConnectionString = "mongodb://mongodb:27017/iotsharp-cap",
            EventBusMqConnectionString = "amqp://guest:guest@rabbitmq:5672"
        };

        var settings = profile.ToHostSettings();

        Assert.Equal("PostgreSql", settings["DataBase"]);
        Assert.Equal("InfluxDB", settings["TelemetryStorage"]);
        Assert.Equal("CAP", settings["EventBus"]);
        Assert.Equal("MongoDB", settings["EventBusStore"]);
        Assert.Equal("RabbitMQ", settings["EventBusMQ"]);
        Assert.Equal("mongodb://mongodb:27017/iotsharp-cap", settings["ConnectionStrings:EventBusStore"]);
        Assert.Equal("amqp://guest:guest@rabbitmq:5672", settings["ConnectionStrings:EventBusMQ"]);
        Assert.True(profile.SupportsExternalEventBus);
    }

    [Fact]
    public void CurrentAppSettingsModel_DoesNotYetExposeRedisOrSonnetDbAsCapPersistenceStore()
    {
        var supportedStores = Enum.GetNames<EventBusStore>();

        Assert.Contains(nameof(EventBusStore.MongoDB), supportedStores);
        Assert.DoesNotContain("Redis", supportedStores);
        Assert.DoesNotContain("SonnetDB", supportedStores);
        Assert.Contains(nameof(EventBusMQ.RedisStreams), Enum.GetNames<EventBusMQ>());
    }
}
