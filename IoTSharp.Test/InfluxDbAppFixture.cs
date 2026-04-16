#nullable enable

using IoTSharp.Contracts;
using System.Threading.Tasks;
using Testcontainers.InfluxDb;
using Testcontainers.PostgreSql;

namespace IoTSharp.Test
{
    public sealed class InfluxDbAppFixture : AppInstance
    {
        private const string Org = "iotsharp";
        private const string Bucket = "iotsharp-bucket";
        private const string AdminToken = "WUhUUUJB0JMIwavPQ4cl-euwnv-B1NC9vHe3fes9NaQ18-D5B37ngIWuTVYPqrIrnuNB2K6halXionzg6K1eyQ==";

        private PostgreSqlContainer? _dbContainer;
        private InfluxDbContainer? _tsdbContainer;

        protected override async Task InitializeAppAsync()
        {
            _dbContainer = new PostgreSqlBuilder().Build();
            _tsdbContainer = new InfluxDbBuilder()
                .WithOrganization(Org)
                .WithBucket(Bucket)
                .WithAdminToken(AdminToken)
                .Build();

            await _dbContainer.StartAsync(TestCancellationToken);
            await _tsdbContainer.StartAsync(TestCancellationToken);
            await InitializeApplicationAsync(
                _dbContainer.GetConnectionString(),
                $"http://{_tsdbContainer.GetAddress()}/?org={Org}&bucket={Bucket}&token={AdminToken}",
                DataBaseType.PostgreSql,
                TelemetryStorage.InfluxDB);
        }

        protected override async Task DisposeTestResourcesAsync()
        {
            if (_dbContainer is not null)
            {
                await _dbContainer.StopAsync(TestCancellationToken);
                await _dbContainer.DisposeAsync();
            }

            if (_tsdbContainer is not null)
            {
                await _tsdbContainer.StopAsync(TestCancellationToken);
                await _tsdbContainer.DisposeAsync();
            }
        }
    }
}