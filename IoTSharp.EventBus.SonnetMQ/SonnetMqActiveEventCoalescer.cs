using System.Collections.Concurrent;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IoTSharp.EventBus.SonnetMQ;

public sealed class SonnetMqActiveEventCoalescer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SonnetMqActiveEventCoalescer> _logger;
    private readonly SonnetMqEventBusOptions _options;
    private readonly ConcurrentDictionary<Guid, ActivePublishState> _states = new();
    private readonly ConcurrentDictionary<Guid, TimeoutCacheEntry> _timeoutCache = new();

    public SonnetMqActiveEventCoalescer(
        IServiceScopeFactory scopeFactory,
        IOptions<SonnetMqEventBusOptions> options,
        ILogger<SonnetMqActiveEventCoalescer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask<DeviceActivityStatus?> RecordAsync(
        Guid deviceId,
        ActivityStatus activity,
        CancellationToken cancellationToken = default)
    {
        var eventTimeUtc = DateTime.UtcNow;
        var coalesceWindow = await GetCoalesceWindowAsync(deviceId, cancellationToken).ConfigureAwait(false);
        var state = _states.GetOrAdd(deviceId, static _ => new ActivePublishState());

        lock (state)
        {
            if (!state.HasPublished
                || state.LastActivity != activity
                || eventTimeUtc - state.LastPublishedAtUtc >= coalesceWindow)
            {
                state.HasPublished = true;
                state.LastActivity = activity;
                state.LastPublishedAtUtc = eventTimeUtc;
                state.CoalesceWindow = coalesceWindow;
                return new DeviceActivityStatus(deviceId, activity, eventTimeUtc);
            }

            state.CoalesceWindow = coalesceWindow;
            return null;
        }
    }

    private async ValueTask<TimeSpan> GetCoalesceWindowAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        if (_timeoutCache.TryGetValue(deviceId, out var cached) && cached.ExpiresAtUtc > now)
        {
            return cached.Window;
        }

        var window = TimeSpan.FromSeconds(Math.Max(1, _options.ActiveCoalesceDefaultSeconds));
        try
        {
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var timeoutSeconds = await dbContext.Device
                .AsNoTracking()
                .Where(device => device.Id == deviceId && !device.Deleted)
                .Select(device => (int?)device.Timeout)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (timeoutSeconds is > 0)
            {
                window = TimeSpan.FromSeconds(timeoutSeconds.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load device active coalesce timeout. DeviceId={DeviceId}", deviceId);
        }

        _timeoutCache[deviceId] = new TimeoutCacheEntry(
            window,
            now.AddSeconds(Math.Max(1, _options.ActiveTimeoutCacheSeconds)));
        return window;
    }

    private sealed class ActivePublishState
    {
        public bool HasPublished { get; set; }

        public ActivityStatus LastActivity { get; set; }

        public DateTime LastPublishedAtUtc { get; set; }

        public TimeSpan CoalesceWindow { get; set; }
    }

    private sealed record TimeoutCacheEntry(TimeSpan Window, DateTime ExpiresAtUtc);
}
