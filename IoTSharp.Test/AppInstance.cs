#nullable enable

using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    public abstract class AppInstance : IAsyncLifetime
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private ApiResult<LoginResult>? _loginResult;
        private IHost? _host;
        private Uri? _baseAddress;

        protected InstallDto? InstallDto { get; private set; }

        public IServiceProvider Services => AssertHost().Services;

        public IoTSharpTestProfile? Profile { get; private set; }

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

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType, string? blobStorage = null) =>
            await InitializeApplicationAsync(dbMain, dbTelemetry, dbType, TelemetryStorage.Sharding, EventBusStore.InMemory, blobStorage);

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType, TelemetryStorage telemetry, string? blobStorage = null) =>
            await InitializeApplicationAsync(dbMain, dbTelemetry, dbType, telemetry, EventBusStore.InMemory, blobStorage);

        protected async Task InitializeApplicationAsync(string dbMain, string dbTelemetry, DataBaseType dbType, TelemetryStorage telemetry, EventBusStore eventBus, string? blobStorage = null)
        {
            var profile = new IoTSharpTestProfile
            {
                Name = $"{dbType}-{telemetry}-{eventBus}",
                DataBase = dbType,
                TelemetryStorage = telemetry,
                EventBusStore = eventBus,
                MainConnectionString = dbMain,
                TelemetryConnectionString = dbTelemetry,
                EventBusStoreConnectionString = eventBus == EventBusStore.InMemory ? string.Empty : dbMain,
                BlobStorageConnectionString = blobStorage
            };

            await InitializeApplicationAsync(profile);
        }

        protected async Task InitializeApplicationAsync(IoTSharpTestProfile profile)
        {
            // Quartz 的日志提供者是进程级静态状态；集成测试会顺序启动多个宿主，需避免复用已释放的宿主 LoggerFactory。
            LogContext.SetCurrentLogProvider(NullLoggerFactory.Instance);
            Profile = profile;
            _host = IoTSharp.Program.CreateHostBuilder(Array.Empty<string>())
                .ConfigureWebHost(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.UseUrls("http://127.0.0.1:0");
                    builder.ConfigureAppConfiguration((_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            profile.ToHostSettings().Select(static item => new KeyValuePair<string, string?>(item.Key, item.Value)));
                    });
                })
                .Build();

            await _host.StartAsync(TestCancellationToken);
            _baseAddress = ResolveBaseAddress(_host);

            using var client = CreateClient();
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
            var responseContent = await response.Content.ReadAsStringAsync(TestCancellationToken);
            Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase}: {responseContent}");

            var result = JsonSerializer.Deserialize<ApiResult<InstanceDto>>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(result);
            Assert.True(result.Code == (int)ApiCode.Success || result.Code == (int)ApiCode.AlreadyExists, result.Msg);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Installed);
        }

        public HttpClient CreateClient() => new() { BaseAddress = AssertBaseAddress() };

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

        public async Task<ApiResult<Device>> CreateDeviceAsync(HttpClient client, string? deviceName = null, DeviceType deviceType = DeviceType.Device)
        {
            await AuthorizeClientAsync(client);

            var device = new DevicePostDto
            {
                DeviceType = deviceType,
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

        public async Task<ApiResult<DeviceDetailDto>> GetDeviceDetailAsync(HttpClient client, Guid deviceId)
        {
            await AuthorizeClientAsync(client);
            var result = await client.GetFromJsonAsync<ApiResult<DeviceDetailDto>>($"/api/Devices/{deviceId}", TestCancellationToken);

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal((int)ApiCode.Success, result.Code);
            return result;
        }

        public async Task<string> GetDeviceAccessTokenAsync(HttpClient client, Guid deviceId)
        {
            var result = await GetDeviceDetailAsync(client, deviceId);

            Assert.False(string.IsNullOrWhiteSpace(result.Data!.IdentityId));
            return result.Data.IdentityId;
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
            if (_host is null)
            {
                return;
            }

            await _host.StopAsync(TestCancellationToken);
            _host.Dispose();
            _host = null;
            _baseAddress = null;
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

        private IHost AssertHost()
        {
            Assert.NotNull(_host);
            return _host!;
        }

        private Uri AssertBaseAddress()
        {
            Assert.NotNull(_baseAddress);
            return _baseAddress!;
        }

        private InstallDto AssertInstallDto()
        {
            Assert.NotNull(InstallDto);
            return InstallDto!;
        }

        private static Uri ResolveBaseAddress(IHost host)
        {
            var server = host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
            var address = addresses?.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidOperationException("The test host did not publish a server address.");
            }

            if (address.Contains("[::]", StringComparison.Ordinal))
            {
                address = address.Replace("[::]", "127.0.0.1", StringComparison.Ordinal);
            }

            return new Uri(address);
        }
    }
}
