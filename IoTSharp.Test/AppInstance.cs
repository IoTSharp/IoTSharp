#nullable enable

using Alba;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IoTSharp.Test
{
    public abstract class AppInstance : IAsyncLifetime
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private ApiResult<LoginResult>? _loginResult;

        protected AppInstance(ITestOutputHelper output)
        {
            Output = output;
        }

        protected ITestOutputHelper Output { get; }

        protected IAlbaHost? Host { get; private set; }

        protected InstallDto? InstallDto { get; private set; }

        protected CancellationToken TestCancellationToken => _cancellationTokenSource.Token;

        public async Task InitializeAsync()
        {
            await InitializeAppAsync();
        }

        public async Task DisposeAsync()
        {
            await StopApplicationAsync();
            await DisposeTestResourcesAsync();
            _cancellationTokenSource.Dispose();
        }

        protected abstract Task InitializeAppAsync();

        protected virtual Task DisposeTestResourcesAsync() => Task.CompletedTask;

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType) =>
            await InitializeApplicationAsync(dbMain, dbTelemetry, dbType, TelemetryStorage.Sharding, EventBusStore.InMemory);

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType, TelemetryStorage telemetry) =>
            await InitializeApplicationAsync(dbMain, dbTelemetry, dbType, telemetry, EventBusStore.InMemory);

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType, TelemetryStorage telemetry, EventBusStore eventBus)
        {
            Host = await AlbaHost.For<IoTSharp.Program>(builder =>
            {
                builder.UseEnvironment("Test");
                builder.UseSetting("DataBase", Enum.GetName(dbType));
                builder.UseSetting("EventBusStore", Enum.GetName(eventBus));
                builder.UseSetting("EventBusMQ", Enum.GetName(EventBusMQ.InMemory));
                builder.UseSetting("TelemetryStorage", Enum.GetName(telemetry));
                builder.UseSetting("EventBus", Enum.GetName(EventBusFramework.CAP));
                builder.UseSetting("ConnectionStrings:IoTSharp", dbMain);
                builder.UseSetting("ConnectionStrings:TelemetryStorage", dbTelemetry);
            });

            using var client = Host.GetTestClient();
            InstallDto = new InstallDto
            {
                CustomerEMail = "customer@iotsharp.net",
                CustomerName = "customer_test",
                Email = "admin@iotsharp.net",
                Password = "P@ssw0rd",
                PhoneNumber = "18900000000",
                TenantEMail = "tenant@iotsharp.net",
                TenantName = "tenant_test"
            };

            var response = await client.PostAsJsonAsync("/api/Installer/Install", InstallDto, TestCancellationToken);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadFromJsonAsync<ApiResult<InstanceDto>>(TestCancellationToken);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Installed);
        }

        protected async Task AssertAppIsInstalledAsync()
        {
            var host = AssertHost();
            using var client = host.GetTestClient();
            var result = await client.GetFromJsonAsync<ApiResult<InstanceDto>>("/api/Installer/Instance", TestCancellationToken);

            Assert.NotNull(result);
            Assert.Equal((int)ApiCode.Success, result.Code);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Installed);
        }

        protected async Task AssertAppAccountLoginAsync()
        {
            var host = AssertHost();
            using var client = host.GetTestClient();
            var result = await LoginAsync(client);

            Assert.NotNull(result.Data);
            Assert.True(result.Data.Succeeded);
            Assert.Equal(AssertInstallDto().Email, result.Data.UserName);
        }

        protected async Task AssertAppDevicesCreateAsync()
        {
            var host = AssertHost();
            using var client = host.GetTestClient();
            await LoginAsync(client);

            var device = new DevicePostDto { DeviceType = DeviceType.Device, Name = "test", Timeout = 30 };
            var response = await client.PostAsJsonAsync("/api/Devices", device, TestCancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResult<Device>>(TestCancellationToken);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal("test", result.Data.Name);
            Assert.Equal((int)ApiCode.Success, result.Code);
        }

        protected async Task StopApplicationAsync()
        {
            if (Host is null)
            {
                return;
            }

            await Host.StopAsync(TestCancellationToken);
            Host = null;
        }

        private async Task<ApiResult<LoginResult>> LoginAsync(HttpClient client)
        {
            if (_loginResult is not null)
            {
                return _loginResult;
            }

            var installDto = AssertInstallDto();
            var dto = new LoginDto { UserName = installDto.Email, Password = installDto.Password };
            var response = await client.PostAsJsonAsync("/api/Account/Login", dto, TestCancellationToken);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadFromJsonAsync<ApiResult<LoginResult>>(TestCancellationToken);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Succeeded);
            Assert.Equal(installDto.Email, result.Data.UserName);
            Assert.NotNull(result.Data.Token);
            Assert.False(string.IsNullOrWhiteSpace(result.Data.Token.access_token));

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.Data.Token.access_token}");
            _loginResult = result;

            return result;
        }

        private IAlbaHost AssertHost()
        {
            Assert.NotNull(Host);
            return Host!;
        }

        private InstallDto AssertInstallDto()
        {
            Assert.NotNull(InstallDto);
            return InstallDto!;
        }
    }
}
