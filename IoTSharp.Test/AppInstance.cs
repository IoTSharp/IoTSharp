#nullable enable

using Alba;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    public abstract class AppInstance : IAsyncLifetime
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private ApiResult<LoginResult>? _loginResult;

        protected IAlbaHost? Host { get; private set; }

        protected InstallDto? InstallDto { get; private set; }

        public IServiceProvider Services => AssertHost().Services;

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

        public HttpClient CreateClient() => AssertHost().GetTestClient();

        public async Task AssertAppIsInstalledAsync()
        {
            using var client = CreateClient();
            var result = await client.GetFromJsonAsync<ApiResult<InstanceDto>>("/api/Installer/Instance", TestCancellationToken);

            Assert.NotNull(result);
            Assert.Equal((int)ApiCode.Success, result.Code);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Installed);
        }

        public async Task AssertAppAccountLoginAsync()
        {
            using var client = CreateClient();
            var result = await LoginAsync(client);

            Assert.NotNull(result.Data);
            Assert.True(result.Data.Succeeded);
            Assert.Equal(AssertInstallDto().Email, result.Data.UserName);
        }

        public async Task AssertAppDevicesCreateAsync()
        {
            using var client = CreateClient();
            var result = await CreateDeviceAsync(client);
            Assert.NotNull(result.Data);
            Assert.Equal((int)ApiCode.Success, result.Code);
        }

        public async Task<ApiResult<Device>> CreateDeviceAsync(HttpClient client, string? deviceName = null)
        {
            await AuthorizeClientAsync(client);

            var device = new DevicePostDto
            {
                DeviceType = DeviceType.Device,
                Name = deviceName ?? $"test-{Guid.NewGuid():N}",
                Timeout = 30
            };

            var response = await client.PostAsJsonAsync("/api/Devices", device, TestCancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResult<Device>>(TestCancellationToken);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            return result;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_loginResult?.Data?.Token?.access_token is { Length: > 0 } token)
            {
                return token;
            }

            using var client = CreateClient();
            var loginResult = await LoginAsync(client);
            Assert.NotNull(loginResult.Data);
            Assert.NotNull(loginResult.Data.Token);
            return loginResult.Data.Token.access_token;
        }

        public async Task AuthorizeClientAsync(HttpClient client)
        {
            var token = await GetAccessTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<ApiResult<LoginResult>> LoginAsync(HttpClient client)
        {
            if (_loginResult is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _loginResult.Data!.Token!.access_token);
                return _loginResult;
            }

            var installDto = AssertInstallDto();
            var captcha = SeedCaptcha();
            var dto = new LoginDto
            {
                UserName = installDto.Email,
                Password = installDto.Password,
                CaptchaClientId = captcha.ClientId,
                CaptchaMove = captcha.Move
            };

            var response = await client.PostAsJsonAsync("/api/Account/Login", dto, TestCancellationToken);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadFromJsonAsync<ApiResult<LoginResult>>(TestCancellationToken);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Succeeded, result.Msg);
            Assert.Equal(installDto.Email, result.Data.UserName);
            Assert.NotNull(result.Data.Token);
            Assert.False(string.IsNullOrWhiteSpace(result.Data.Token.access_token));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Data.Token.access_token);
            _loginResult = result;

            return result;
        }

        private (string ClientId, int Move) SeedCaptcha()
        {
            using var scope = Services.CreateScope();
            var caching = scope.ServiceProvider.GetRequiredService<IEasyCachingProvider>();
            var clientId = Guid.NewGuid().ToString("N");
            var move = Random.Shared.Next(32, 256);
            var list = caching.Get<List<ModelCaptchaVertifyItem>>("Captcha").Value ?? new List<ModelCaptchaVertifyItem>();

            list.RemoveAll(item => item.Clientid == clientId);
            list.Add(new ModelCaptchaVertifyItem { Clientid = clientId, Move = move });
            caching.Set("Captcha", list, TimeSpan.FromMinutes(20));

            return (clientId, move);
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
