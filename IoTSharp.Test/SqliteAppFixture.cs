#nullable enable

using IoTSharp.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IoTSharp.Test
{
    public sealed class SqliteAppFixture : AppInstance
    {
        private string? _dataDirectory;

        protected override async Task InitializeAppAsync()
        {
            _dataDirectory = Path.Combine(Path.GetTempPath(), "IoTSharp.Test", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataDirectory);

            await InitializeApplicationAsync(
                $"Data Source={Path.Combine(_dataDirectory, "IoTSharp.db")}",
                $"Data Source={Path.Combine(_dataDirectory, "TelemetryStorage.db")}",
                DataBaseType.Sqlite);
        }

        protected override Task DisposeTestResourcesAsync()
        {
            if (_dataDirectory is not null && Directory.Exists(_dataDirectory))
            {
                try
                {
                    Directory.Delete(_dataDirectory, recursive: true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            return Task.CompletedTask;
        }
    }
}