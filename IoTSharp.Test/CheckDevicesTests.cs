#nullable enable

using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.EventBus;
using IoTSharp.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class CheckDevicesTests
{
    private readonly SqliteAppFixture _fixture;

    public CheckDevicesTests(SqliteAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckInactiveDevicesAsync_WhenRunRepeatedly_PublishesOneTransition()
        => await CheckDevicesTestCase.RunRepeatedlyAsync(_fixture);
}

/// <summary>
/// 验证 SonnetDB provider 下设备离线条件更新可执行且不会重复发布。
/// </summary>
[Collection(IntegrationTestCollectionNames.SonnetDbApplication)]
public sealed class CheckDevicesSonnetDbTests
{
    private readonly SonnetDbAppFixture _fixture;

    public CheckDevicesSonnetDbTests(SonnetDbAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckInactiveDevicesAsync_WhenRunRepeatedly_PublishesOneTransition()
        => await CheckDevicesTestCase.RunRepeatedlyAsync(_fixture);
}

internal static class CheckDevicesTestCase
{
    /// <summary>
    /// 在指定数据库 fixture 上验证离线状态原子切换和重复执行幂等性。
    /// </summary>
    /// <param name="fixture">应用与数据库测试实例。</param>
    public static async Task RunRepeatedlyAsync(AppInstance fixture)
    {
        using var client = fixture.CreateClient();
        var device = await fixture.CreateDeviceAsync(client, $"inactive-check-{Guid.NewGuid():N}");
        var deviceId = device.Data!.Id;
        var now = new DateTime(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc);

        using (var scope = fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.SaveAsync<AttributeLatest>(new Dictionary<string, object>
            {
                [Constants._Active] = true,
                [Constants._LastActivityDateTime] = now.AddMinutes(-5)
            }, deviceId, DataSide.ServerSide);
        }

        var publisher = new RecordingPublisher();
        var job = new CheckDevices(
            NullLogger<CheckDevices>.Instance,
            fixture.Services.GetRequiredService<IServiceScopeFactory>(),
            publisher);

        var firstCount = await job.CheckInactiveDevicesAsync(now);
        var secondCount = await job.CheckInactiveDevicesAsync(now.AddMinutes(1));

        Assert.Equal(1, firstCount);
        Assert.Equal(0, secondCount);
        var transition = Assert.Single(publisher.ActivityTransitions);
        Assert.Equal(deviceId, transition.DeviceId);
        Assert.Equal(ActivityStatus.Inactivity, transition.Status);

        using var verificationScope = fixture.Services.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var active = await verificationContext.AttributeLatest
            .AsNoTracking()
            .SingleAsync(item => item.DeviceId == deviceId && item.KeyName == Constants._Active);
        Assert.False(active.Value_Boolean);
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public List<(Guid DeviceId, ActivityStatus Status)> ActivityTransitions { get; } = new();

        public Task<EventBusMetrics> GetMetrics() => Task.FromResult(new EventBusMetrics());

        public Task PublishCreateDevice(Guid devid) => Task.CompletedTask;

        public Task PublishDeleteDevice(Guid devid) => Task.CompletedTask;

        public Task PublishAttributeData(PlayloadData msg) => Task.CompletedTask;

        public Task PublishTelemetryData(PlayloadData msg) => Task.CompletedTask;

        public Task PublishConnect(Guid devid, ConnectStatus devicestatus) => Task.CompletedTask;

        public Task PublishActive(Guid devid, ActivityStatus activity)
        {
            ActivityTransitions.Add((devid, activity));
            return Task.CompletedTask;
        }

        public Task PublishDeviceAlarm(CreateAlarmDto alarmDto) => Task.CompletedTask;
    }
}
