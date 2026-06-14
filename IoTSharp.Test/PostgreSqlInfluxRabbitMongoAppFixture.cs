#nullable enable

using System.Threading.Tasks;
using IoTSharp.Contracts;
using Testcontainers.InfluxDb;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IoTSharp.Test;

public sealed class PostgreSqlInfluxRabbitMongoAppFixture : AppInstance
{
    private const string InfluxOrg = "iotsharp";
    private const string InfluxBucket = "iotsharp-test";
    private const string InfluxToken = "iotsharp-test-token-change-me";

    private PostgreSqlContainer? _postgres;
    private InfluxDbContainer? _influx;
    private MongoDbContainer? _mongo;
    private RabbitMqContainer? _rabbit;

    protected override async Task InitializeAppAsync()
    {
        _postgres = new PostgreSqlBuilder().Build();
        _influx = new InfluxDbBuilder()
            .WithOrganization(InfluxOrg)
            .WithBucket(InfluxBucket)
            .WithAdminToken(InfluxToken)
            .Build();
        _mongo = new MongoDbBuilder().Build();
        _rabbit = new RabbitMqBuilder().Build();

        await _postgres.StartAsync(TestCancellationToken);
        await _influx.StartAsync(TestCancellationToken);
        await _mongo.StartAsync(TestCancellationToken);
        await _rabbit.StartAsync(TestCancellationToken);

        await InitializeApplicationAsync(new IoTSharpTestProfile
        {
            Name = "postgresql-influxdb-rabbitmq-mongodb",
            DataBase = DataBaseType.PostgreSql,
            TelemetryStorage = TelemetryStorage.InfluxDB,
            EventBus = EventBusFramework.CAP,
            EventBusStore = EventBusStore.MongoDB,
            EventBusMQ = EventBusMQ.RabbitMQ,
            MainConnectionString = _postgres.GetConnectionString(),
            TelemetryConnectionString = $"{_influx.GetAddress()}/?org={InfluxOrg}&bucket={InfluxBucket}&token={InfluxToken}",
            EventBusStoreConnectionString = _mongo.GetConnectionString(),
            EventBusMqConnectionString = _rabbit.GetConnectionString()
        });
    }

    protected override async Task DisposeTestResourcesAsync()
    {
        if (_rabbit is not null)
        {
            await _rabbit.DisposeAsync();
        }

        if (_mongo is not null)
        {
            await _mongo.DisposeAsync();
        }

        if (_influx is not null)
        {
            await _influx.DisposeAsync();
        }

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }
}
