using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Controllers;

[ApiController]
[Authorize]
[Route("api/DashBoard/[action]")]
public sealed class DashBoardController(ApplicationDbContext db, ILogger<DashBoardController> logger) : ControllerBase
{
    private const int MaxSampleRows = 100_000;
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
    private readonly TimeProvider _clock = TimeProvider.System;

    internal DashBoardController(ApplicationDbContext db, ILogger<DashBoardController> logger, TimeProvider clock)
        : this(db, logger)
    {
        _clock = clock;
    }

    /// <summary>按当前用户所属租户查询消息总线趋势。</summary>
    /// <param name="range">时间范围，支持 24h 和 7d，最后一个桶包含当前时间。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    /// <returns>从旧到新排列的时间桶。每项缺失采样返回空值，有效零值保留。</returns>
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ProducesResponseType(typeof(ApiResult<DashboardTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResult<DashboardTrendDto>>> Trend([FromQuery] string range, CancellationToken cancellationToken)
    {
        var (bucketMinutes, bucketCount) = range switch
        {
            "24h" => (60, 24),
            "7d" => (360, 28),
            _ => (0, 0)
        };
        if (bucketCount == 0)
            return BadRequest(new ApiResult(ApiCode.InValidData, "时间范围须为 24h 或 7d。"));

        if (!Guid.TryParse(User.FindFirstValue(IoTSharpClaimTypes.Tenant), out var tenantId) || tenantId == Guid.Empty)
            return Forbid();

        var nowUtc = _clock.GetUtcNow().UtcDateTime;
        var currentBucket = AlignBucket(nowUtc, bucketMinutes);
        var firstBucket = currentBucket.AddMinutes(-(bucketCount - 1) * bucketMinutes);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(QueryTimeout);
        var readToken = timeout.Token;
        try
        {
            if (!await db.Tenant.AnyAsync(tenant => tenant.Id == tenantId && !tenant.Deleted, readToken))
                return Forbid();

            var tenantSamples = db.Set<DashboardTrendSample>().AsNoTracking().Where(sample => sample.TenantId == tenantId);
            var samples = await tenantSamples
                .Where(sample => sample.StartUtc >= firstBucket && sample.StartUtc <= nowUtc)
                .OrderBy(sample => sample.StartUtc)
                .Take(MaxSampleRows + 1)
                .ToListAsync(readToken);
            if (samples.Count > MaxSampleRows)
            {
                logger.LogWarning("消息总线趋势查询超过采样行数上限。TenantId={TenantId}, Range={Range}", tenantId, range);
                return TrendUnavailable();
            }

            var samplesByBucket = samples.ToLookup(sample => AlignBucket(sample.StartUtc, bucketMinutes));
            var points = new DashboardTrendPointDto[bucketCount];
            for (int index = 0; index < points.Length; index++)
            {
                readToken.ThrowIfCancellationRequested();
                var startUtc = firstBucket.AddMinutes(index * bucketMinutes);
                points[index] = ToPoint(startUtc, bucketMinutes, nowUtc, samplesByBucket[startUtc]);
            }

            string state = "ready";
            if (!points.Any(point => point.Received.HasValue || point.Processed.HasValue || point.Telemetry.HasValue
                || point.Attributes.HasValue || point.OnlineDevices.HasValue))
            {
                var hasDevices = await db.Device.AnyAsync(device => device.TenantId == tenantId && !device.Deleted, readToken);
                var hasOlderSamples = hasDevices && await tenantSamples.AnyAsync(sample => sample.StartUtc < firstBucket, readToken);
                state = hasDevices && !hasOlderSamples ? "collecting" : "empty";
            }

            return Ok(new ApiResult<DashboardTrendDto>(new DashboardTrendDto(range, bucketMinutes, state, points)));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "读取消息总线趋势失败。TenantId={TenantId}, Range={Range}", tenantId, range);
            return TrendUnavailable();
        }
    }

    /// <summary>将时间戳对齐到对应的 UTC 时间桶起点。</summary>
    private static DateTime AlignBucket(DateTime timestamp, int bucketMinutes)
    {
        long bucketTicks = TimeSpan.FromMinutes(bucketMinutes).Ticks;
        return new DateTime(timestamp.Ticks - timestamp.Ticks % bucketTicks, DateTimeKind.Utc);
    }

    /// <summary>各计数按采样分钟累加，在线设备取时间桶内最后一份快照。</summary>
    private static DashboardTrendPointDto ToPoint(
        DateTime startUtc, int bucketMinutes, DateTime nowUtc, IEnumerable<DashboardTrendSample> samples)
    {
        var received = SumSamples(samples, sample => sample.Received);
        var processed = SumSamples(samples, sample => sample.Processed);
        var telemetry = SumSamples(samples, sample => sample.Telemetry);
        var attributes = SumSamples(samples, sample => sample.Attributes);
        var bucketEndUtc = startUtc.AddMinutes(bucketMinutes);
        var onlineSamples = samples.Where(sample => sample.OnlineDevices.HasValue && sample.SnapshotUtc.HasValue
            && sample.SnapshotUtc.Value >= startUtc && sample.SnapshotUtc.Value < bucketEndUtc
            && sample.SnapshotUtc.Value <= nowUtc);
        var lastOnline = onlineSamples.MaxBy(sample => sample.SnapshotUtc);
        bool partial = bucketEndUtc > nowUtc || received.Minutes < bucketMinutes || processed.Minutes < bucketMinutes
            || telemetry.Minutes < bucketMinutes || attributes.Minutes < bucketMinutes
            || onlineSamples.Select(sample => sample.StartUtc).Distinct().Count() < bucketMinutes;
        return new DashboardTrendPointDto(startUtc, received.Value, processed.Value, telemetry.Value, attributes.Value, lastOnline?.OnlineDevices, partial);
    }

    /// <summary>汇总单个计数项，保留空桶与已采样零值的差异。</summary>
    private static (long? Value, int Minutes) SumSamples(
        IEnumerable<DashboardTrendSample> samples, Func<DashboardTrendSample, long?> selector)
    {
        var minutes = new HashSet<DateTime>();
        long sum = 0;
        foreach (var sample in samples)
        {
            if (selector(sample) is long value)
            {
                sum = checked(sum + value);
                minutes.Add(sample.StartUtc);
            }
        }

        return (minutes.Count == 0 ? null : sum, minutes.Count);
    }

    /// <summary>返回已脱敏的查询失败响应，避免客户端将故障呈现为零值。</summary>
    private ObjectResult TrendUnavailable()
        => StatusCode(StatusCodes.Status503ServiceUnavailable, new ApiResult(ApiCode.Exception, "暂时无法查询消息总线趋势。"));
}
