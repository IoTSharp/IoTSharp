#nullable enable

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.SonnetDB;

namespace IoTSharp.Test;

public class SonnetDbAppFixture : AppInstance
{
    internal const string ConfigurationLeaseQualificationVariable = "IOTSHARP_SONNETDB_LEASE_QUALIFICATION";
    private const string AdminToken = "iotsharp_sonnetdb_test_token_change_me";
    private const string DefaultImage = "ghcr.io/iotsharp/sonnetdb:5.2.0";

    private SonnetDbContainer? _sonnetDb;

    /// <summary>
    /// 启动发布版 SonnetDB 测试容器，可通过环境变量选择另一个明确的测试镜像。
    /// </summary>
    protected override async Task InitializeAppAsync()
    {
        _sonnetDb = CreateSonnetDbContainer();

        using var startup = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        startup.CancelAfter(TimeSpan.FromMinutes(5));
        await _sonnetDb.StartAsync(startup.Token);

        var main = _sonnetDb.GetConnectionString("iotsharp");
        var telemetry = $"{_sonnetDb.GetConnectionString("telemetry")};Measurement=TelemetryData;AutoCreate=true";
        var events = _sonnetDb.GetConnectionString("events");
        var cache = _sonnetDb.GetConnectionString("cache");
        var objects = _sonnetDb.GetConnectionString("objects");
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var mqttPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        await InitializeApplicationAsync(new IoTSharpTestProfile
        {
            Name = "sonnetdb-container",
            DataBase = DataBaseType.SonnetDB,
            TelemetryStorage = TelemetryStorage.SonnetDB,
            EventBus = EventBusFramework.SonnetMQ,
            EventBusStore = EventBusStore.InMemory,
            EventBusMQ = EventBusMQ.SonnetMQ,
            CachingUseIn = CachingUseIn.SonnetDB,
            PersistQuartzJobs = true,
            MqttBrokerPort = mqttPort,
            MainConnectionString = main,
            TelemetryConnectionString = telemetry,
            EventBusMqConnectionString = events,
            CachingUseSonnetDBConnectionString = cache,
            BlobStorageConnectionString = "sonnetdb://blob?bucket=iotsharp-blob-storage&connectionString=" + Uri.EscapeDataString(objects)
        }, ConfigureLeaseQualificationServices);
    }

    /// <summary>
    /// 使用同一个业务库重启 IoTSharp，以验证设备和调度状态的恢复。
    /// </summary>
    public async Task RestartApplicationAsync()
    {
        var profile = Profile ?? throw new InvalidOperationException("The SonnetDB fixture has not started.");
        await StopApplicationAsync();
        await InitializeApplicationAsync(profile, ConfigureLeaseQualificationServices);
    }

    /// <summary>在需要资格验证的测试宿主中注册远程租约候选。</summary>
    protected virtual void ConfigureLeaseQualificationServices(IServiceCollection services)
    {
        if (Environment.GetEnvironmentVariable(ConfigurationLeaseQualificationVariable) == "1")
        {
            services.Replace(ServiceDescriptor.Scoped(sp => new ConfigurationOperationLeaseService(
                sp.GetRequiredService<ApplicationDbContext>(), qualifySonnetDb: true)));
        }
    }

    /// <summary>
    /// 释放本测试夹具创建的容器及其临时资源。
    /// </summary>
    protected override async Task DisposeTestResourcesAsync()
    {
        if (_sonnetDb is not null)
        {
            await _sonnetDb.DisposeAsync();
        }
    }

    /// <summary>
    /// 使用固定发布镜像创建容器，不依赖 SonnetDB 源码检出或本地 Docker 构建。
    /// </summary>
    /// <returns>由测试夹具负责释放的临时容器。</returns>
    private static SonnetDbContainer CreateSonnetDbContainer()
    {
        var configuredImage = Environment.GetEnvironmentVariable("SONNETDB_TEST_IMAGE");
        var image = string.IsNullOrWhiteSpace(configuredImage) ? DefaultImage : configuredImage.Trim();
        return new SonnetDbBuilder(image)
            .WithAdminToken(AdminToken)
            .WithDatabase("iotsharp")
            .Build();
    }
}
