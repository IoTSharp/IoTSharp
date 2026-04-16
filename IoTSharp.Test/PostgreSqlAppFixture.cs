#nullable enable

using IoTSharp.Contracts;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace IoTSharp.Test
{
    public sealed class PostgreSqlAppFixture : AppInstance
    {
        private PostgreSqlContainer? _dbContainer;

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
    }
}