using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.InMemory;
using IoTSharp.EventBus;
using IoTSharp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

public sealed class DashboardTrendCollectorTests
{
    [Fact]
    public void CompleteMinutesKeepPublishedAndProcessedSeparateAndPreserveGaps()
    {
        var minute = new DateTimeOffset(2026, 11, 25, 5, 0, 0, TimeSpan.Zero);
        var clock = new TestClock(minute.AddSeconds(30));
        var counters = new DashboardTrendCounters(clock);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        counters.RecordReceived(first);
        clock.Now = minute.AddMinutes(1);
        Assert.Null(counters.TakeCompletedMinute());
        counters.RecordReceived(first, DashboardTrendCounters.ReportKind.Telemetry);
        counters.RecordReceived(first, DashboardTrendCounters.ReportKind.Attribute);
        counters.RecordReceived(second);
        counters.RecordProcessed(new[] { new PlayloadData { DeviceId = second }, new PlayloadData { DeviceId = second } });
        clock.Now = minute.AddMinutes(2);

        var completed = counters.TakeCompletedMinute().Value;
        Assert.Equal(minute.UtcDateTime.AddMinutes(1), completed.StartUtc);
        Assert.Equal(new DashboardTrendCounters.Counts(2, 0, 1, 1), completed.Devices[first]);
        Assert.Equal(new DashboardTrendCounters.Counts(1, 2, 0, 0), completed.Devices[second]);
        Assert.Null(counters.TakeCompletedMinute());

        clock.Now = minute.AddMinutes(3);
        Assert.Empty(counters.TakeCompletedMinute().Value.Devices);
        clock.Now = minute.AddMinutes(5);
        Assert.Null(counters.TakeCompletedMinute());
        clock.Now = minute.AddMinutes(6);
        Assert.Empty(counters.TakeCompletedMinute().Value.Devices);
    }

    [Fact(Timeout = 15000)]
    public async Task CollectorPersistsCountsAndRealOnlineSnapshotsByTenant()
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkInMemoryDatabase();
        services.AddSingleton<IDataBaseModelBuilderOptions, InMemoryModelBuilderOptions>();
        var database = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>((provider, options) =>
            options.UseInMemoryDatabase(database).UseInternalServiceProvider(provider));
        await using var provider = services.BuildServiceProvider();
        await using var seed = provider.CreateAsyncScope();
        var db = seed.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = new Tenant();
        var other = new Tenant();
        var deletedTenant = new Tenant { Deleted = true };
        db.Tenant.AddRange(tenant, other, deletedTenant);
        var online = new Device { Id = Guid.NewGuid(), Name = "online", TenantId = tenant.Id };
        var deleted = new Device { Id = Guid.NewGuid(), Name = "deleted", TenantId = tenant.Id, Deleted = true };
        var clientAttribute = new Device { Id = Guid.NewGuid(), Name = "client", TenantId = other.Id };
        db.Device.AddRange(online, deleted, clientAttribute);
        db.AttributeLatest.AddRange(Active(online, DataSide.ServerSide), Active(deleted, DataSide.ServerSide),
            Active(clientAttribute, DataSide.ClientSide));
        await db.SaveChangesAsync();

        var start = DashboardTrendCounters.MinuteStart(DateTime.UtcNow.AddMinutes(-5));
        using var collector = new DashboardTrendCollector(new DashboardTrendCounters(),
            provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<DashboardTrendCollector>.Instance);
        await collector.StoreSampleAsync(start, new Dictionary<Guid, DashboardTrendCounters.Counts>
        {
            [online.Id] = new(3, 1, 2, 1),
            [deleted.Id] = new(0, 1, 0, 0),
            [Guid.NewGuid()] = new(100, 100, 100, 100)
        }, CancellationToken.None);

        var samples = await db.Set<DashboardTrendSample>().AsNoTracking().ToListAsync();
        Assert.Equal(4, samples.Count);
        var ownCounters = Assert.Single(samples, sample => sample.TenantId == tenant.Id && sample.StartUtc == start);
        Assert.Equal((3L, 2L, 2L, 1L), (ownCounters.Received, ownCounters.Processed, ownCounters.Telemetry, ownCounters.Attributes));
        Assert.Null(ownCounters.OnlineDevices);
        var ownSnapshot = Assert.Single(samples, sample => sample.TenantId == tenant.Id && sample.SnapshotUtc.HasValue);
        Assert.Equal(1, ownSnapshot.OnlineDevices);
        Assert.Equal(DashboardTrendCounters.MinuteStart(ownSnapshot.SnapshotUtc.Value), ownSnapshot.StartUtc);
        Assert.True(ownSnapshot.StartUtc > start);
        Assert.Equal(0, Assert.Single(samples, sample => sample.TenantId == other.Id && sample.SnapshotUtc.HasValue).OnlineDevices);
        Assert.Equal(0L, Assert.Single(samples, sample => sample.TenantId == other.Id && sample.StartUtc == start).Received);
        Assert.DoesNotContain(samples, sample => sample.TenantId == deletedTenant.Id);
    }

    private static AttributeLatest Active(Device device, DataSide side)
        => new() { DeviceId = device.Id, DataSide = side, KeyName = Constants._Active, Value_Boolean = true };

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
