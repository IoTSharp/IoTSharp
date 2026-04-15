#nullable enable

using IoTSharp.Contracts;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace IoTSharp.Test
{
    public sealed class AppWithPostgreSqlTest : AppInstance
    {
        private PostgreSqlContainer? _dbContainer;

        public AppWithPostgreSqlTest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override async Task InitializeAppAsync()
        {
            _dbContainer = new PostgreSqlBuilder().Build();
            await _dbContainer.StartAsync(TestCancellationToken);
            await InitializeApplicationAsync(_dbContainer.GetConnectionString(), _dbContainer.GetConnectionString(), DataBaseType.PostgreSql);
        }

        protected override async Task DisposeTestResourcesAsync()
        {
            if (_dbContainer is not null)
            {
                await _dbContainer.DisposeAsync();
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
