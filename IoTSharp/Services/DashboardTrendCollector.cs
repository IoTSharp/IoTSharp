using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services;

/// <summary>每个宿主持久化自己的消息计数，并定时保存按租户统计的在线设备总量。</summary>
internal sealed class DashboardTrendCollector(
    DashboardTrendCounters counters,
    IServiceScopeFactory scopeFactory,
    ILogger<DashboardTrendCollector> logger) : BackgroundService
{
    internal static readonly TimeSpan Retention = TimeSpan.FromDays(8);
    private static readonly TimeSpan DatabaseTimeout = TimeSpan.FromSeconds(20);
    private readonly Guid _counterCollectorId = Guid.NewGuid();
    private DateTime _nextCleanupUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var sample = counters.TakeCompletedMinute();
            if (sample is not { } completed)
                continue;

            try
            {
                await StoreSampleAsync(completed.StartUtc, completed.Devices, stoppingToken);
                if (DateTime.UtcNow >= _nextCleanupUtc)
                {
                    _nextCleanupUtc = DateTime.UtcNow.AddHours(1);
                    await DeleteExpiredAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "保存仪表盘趋势或清理过期数据失败。StartUtc={StartUtc}", completed.StartUtc);
            }
        }
    }

    /// <summary>将观测到的设备计数按真实租户归属保存，另外保存当时的在线设备快照。</summary>
    internal async Task StoreSampleAsync(DateTime startUtc,
        IReadOnlyDictionary<Guid, DashboardTrendCounters.Counts> countsByDevice,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(DatabaseTimeout);

        var tenants = await db.Tenant.AsNoTracking().Where(tenant => !tenant.Deleted)
            .Select(tenant => tenant.Id).ToListAsync(timeout.Token);
        var devices = await db.Device.AsNoTracking().Where(device => device.TenantId.HasValue)
            .Select(device => new { device.Id, TenantId = device.TenantId.Value, device.Deleted })
            .ToDictionaryAsync(device => device.Id, timeout.Token);

        var totals = tenants.ToDictionary(tenantId => tenantId, _ => default(DashboardTrendCounters.Counts));
        foreach (var (deviceId, counts) in countsByDevice)
        {
            if (devices.TryGetValue(deviceId, out var device) && totals.TryGetValue(device.TenantId, out var prior))
                totals[device.TenantId] = prior + counts;
        }

        var samples = db.Set<DashboardTrendSample>();
        samples.AddRange(totals.Select(tenant => new DashboardTrendSample
        {
            TenantId = tenant.Key,
            StartUtc = startUtc,
            CollectorId = _counterCollectorId,
            Received = tenant.Value.Received,
            Processed = tenant.Value.Processed,
            Telemetry = tenant.Value.Telemetry,
            Attributes = tenant.Value.Attributes
        }));

        try
        {
            using var onlineTimeout = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            onlineTimeout.CancelAfter(TimeSpan.FromSeconds(5));
            var online = await db.AttributeLatest.AsNoTracking()
                .Where(attribute => attribute.DataSide == DataSide.ServerSide
                    && attribute.KeyName == Constants._Active && attribute.Value_Boolean == true)
                .Select(attribute => attribute.DeviceId).ToListAsync(onlineTimeout.Token);
            var snapshotUtc = DateTime.UtcNow;
            var snapshotMinute = DashboardTrendCounters.MinuteStart(snapshotUtc);
            var snapshotId = Guid.NewGuid();
            var onlineByTenant = online.Distinct()
                .Where(id => devices.TryGetValue(id, out var device) && !device.Deleted)
                .CountBy(id => devices[id].TenantId).ToDictionary();

            samples.AddRange(tenants.Select(tenantId => new DashboardTrendSample
            {
                TenantId = tenantId,
                StartUtc = snapshotMinute,
                CollectorId = snapshotId,
                OnlineDevices = onlineByTenant.GetValueOrDefault(tenantId),
                SnapshotUtc = snapshotUtc
            }));
        }
        catch (Exception exception) when (!timeout.IsCancellationRequested)
        {
            logger.LogWarning(exception, "采集在线设备快照失败。StartUtc={StartUtc}", startUtc);
        }

        await db.SaveChangesAsync(timeout.Token);
    }

    private async Task DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(DatabaseTimeout);

        var cutoff = DateTime.UtcNow - Retention;
        var expired = db.Set<DashboardTrendSample>().Where(sample => sample.StartUtc < cutoff);
        if (db.Database.IsRelational())
            await expired.ExecuteDeleteAsync(timeout.Token);
        else
        {
            db.RemoveRange(await expired.Take(10000).ToListAsync(timeout.Token));
            await db.SaveChangesAsync(timeout.Token);
        }
    }
}
