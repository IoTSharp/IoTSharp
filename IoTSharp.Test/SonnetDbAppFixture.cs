#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using Testcontainers.SonnetDB;

namespace IoTSharp.Test;

public sealed class SonnetDbAppFixture : AppInstance
{
    private const string AdminToken = "iotsharp_sonnetdb_test_token_change_me";
    private const string DefaultImage = "iotsharp/sonnetdb:5.0.0";

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

        await InitializeApplicationAsync(new IoTSharpTestProfile
        {
            Name = "sonnetdb-container",
            DataBase = DataBaseType.SonnetDB,
            TelemetryStorage = TelemetryStorage.SonnetDB,
            EventBus = EventBusFramework.SonnetMQ,
            EventBusStore = EventBusStore.InMemory,
            EventBusMQ = EventBusMQ.SonnetMQ,
            CachingUseIn = CachingUseIn.SonnetDB,
            MainConnectionString = main,
            TelemetryConnectionString = telemetry,
            EventBusMqConnectionString = events,
            CachingUseSonnetDBConnectionString = cache,
            BlobStorageConnectionString = "sonnetdb://blob?bucket=iotsharp-blob-storage&connectionString=" + Uri.EscapeDataString(objects)
        });
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
