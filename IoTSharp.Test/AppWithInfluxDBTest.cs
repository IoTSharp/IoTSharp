#nullable enable

using IoTSharp.Contracts;
using System.Threading.Tasks;
using Testcontainers.InfluxDb;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;
namespace IoTSharp.Test
{
    public sealed class AppWithInfluxDBTest : AppInstance
    {
        private const string _org = "iotsharp";
        private const string _bucket = "iotsharp-bucket";
        private const string _adminToken = "WUhUUUJB0JMIwavPQ4cl-euwnv-B1NC9vHe3fes9NaQ18-D5B37ngIWuTVYPqrIrnuNB2K6halXionzg6K1eyQ==";
        private PostgreSqlContainer? _dbContainer;
        private InfluxDbContainer? _tsdbContainer;

        public AppWithInfluxDBTest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override async Task InitializeAppAsync()
        {
            _dbContainer = new PostgreSqlBuilder().Build();
            _tsdbContainer = new InfluxDbBuilder()
                .WithOrganization(_org)
                .WithBucket(_bucket)
                .WithAdminToken(_adminToken)
                .Build();

            await _dbContainer.StartAsync(TestCancellationToken);
            await _tsdbContainer.StartAsync(TestCancellationToken);
            await InitializeApplicationAsync(
                _dbContainer.GetConnectionString(),
                $"http://{_tsdbContainer.GetAddress()}/?org={_org}&bucket={_bucket}&token={_adminToken}",
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

        [Fact]
        public Task AppIsInstalled() => AssertAppIsInstalledAsync();

        [Fact]
        public Task AppAccountLogin() => AssertAppAccountLoginAsync();

        [Fact]
        public Task AppDevicesCreate() => AssertAppDevicesCreateAsync();
    }
}
