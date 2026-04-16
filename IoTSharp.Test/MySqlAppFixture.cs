#nullable enable

using IoTSharp.Contracts;
using System.Threading.Tasks;
using Testcontainers.MySql;

namespace IoTSharp.Test
{
    public sealed class MySqlAppFixture : AppInstance
    {
        private MySqlContainer? _dbContainer;

        protected override async Task InitializeAppAsync()
        {
            _dbContainer = new MySqlBuilder().Build();
            await _dbContainer.StartAsync(TestCancellationToken);
            await InitializeApplicationAsync(_dbContainer.GetConnectionString(), _dbContainer.GetConnectionString(), DataBaseType.MySql);
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