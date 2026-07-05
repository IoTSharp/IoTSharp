using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class CachingExtension
    {
        private static readonly TimeSpan KanbanCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan KanbanStaleCacheDuration = TimeSpan.FromHours(1);
        private static readonly TimeSpan KanbanQueryTimeout = TimeSpan.FromSeconds(8);

        public static HomeKanbanDto GetKanBanCache(this IEasyCachingProvider _caching, Guid tid, ApplicationDbContext _context)
        {


            var today = DateTime.Today.ToUniversalTime();
            var kbc = _caching.Get($"{nameof(HomeKanbanDto)}{tid}", () =>
            {
                HomeKanbanDto m = new();
                m.DeviceCount = _context.Device.Count(c => c.Tenant.Id == tid && !c.Deleted);
                m.EventCount = _context.BaseEvents.Count(c => c.Tenant.Id == tid && c.CreaterDateTime > today && c.EventStaus > -1);
                m.OnlineDeviceCount = 0;
                m.AttributesDataCount = 0;
                m.AlarmsCount = _context.Alarms.Count(c => c.Tenant.Id == tid && c.StartDateTime > today);
                var tenantValue = tid.ToString();
                var tuc = from t in _context.UserClaims where t.ClaimType == IoTSharpClaimTypes.Tenant && t.ClaimValue == tenantValue select t;
                var uq = from u in _context.Users where tuc.Any(c => c.UserId == u.Id) select u;
                m.UserCount = uq.Count();
                m.ProductCount = _context.Products.Count(c => c.Tenant.Id == tid && !c.Deleted);
                m.RulesCount = _context.FlowRules.Count(c => c.Tenant.Id == tid && c.RuleStatus > -1);
                return m;
            }, KanbanCacheDuration);
            return kbc.Value;
        }

        public static async Task<HomeKanbanDto> GetKanBanCacheAsync(this IEasyCachingProvider _caching, Guid tid, ApplicationDbContext _context, CancellationToken cancellationToken = default, ILogger logger = null)
        {
            var cacheKey = $"{nameof(HomeKanbanDto)}{tid}";
            var staleCacheKey = $"{cacheKey}:stale";
            var cached = await _caching.GetAsync<HomeKanbanDto>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cached.HasValue)
            {
                return cached.Value;
            }

            var stale = await _caching.GetAsync<HomeKanbanDto>(staleCacheKey, cancellationToken).ConfigureAwait(false);
            var fallback = stale.HasValue ? stale.Value : new HomeKanbanDto();
            var today = DateTime.Today.ToUniversalTime();
            var tenantValue = tid.ToString();
            var allSucceeded = true;
            HomeKanbanDto m = new();

            var deviceCountResult = await ExecuteKanbanQueryAsync(
                "DeviceCount",
                token => _context.Device.CountAsync(c => c.Tenant.Id == tid && !c.Deleted, token),
                fallback.DeviceCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.DeviceCount = deviceCountResult.Value;
            allSucceeded &= deviceCountResult.Success;

            m.OnlineDeviceCount = fallback.OnlineDeviceCount;
            m.AttributesDataCount = fallback.AttributesDataCount;

            var eventResult = await ExecuteKanbanQueryAsync(
                "EventCount",
                token => _context.BaseEvents.CountAsync(c => c.Tenant.Id == tid && c.CreaterDateTime > today && c.EventStaus > -1, token),
                fallback.EventCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.EventCount = eventResult.Value;
            allSucceeded &= eventResult.Success;

            var alarmsResult = await ExecuteKanbanQueryAsync(
                "AlarmsCount",
                token => _context.Alarms.CountAsync(c => c.Tenant.Id == tid && c.StartDateTime > today, token),
                fallback.AlarmsCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.AlarmsCount = alarmsResult.Value;
            allSucceeded &= alarmsResult.Success;

            var userResult = await ExecuteKanbanQueryAsync(
                "UserCount",
                token =>
                {
                    var tuc = from t in _context.UserClaims where t.ClaimType == IoTSharpClaimTypes.Tenant && t.ClaimValue == tenantValue select t;
                    var uq = from u in _context.Users where tuc.Any(c => c.UserId == u.Id) select u;
                    return uq.CountAsync(token);
                },
                fallback.UserCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.UserCount = userResult.Value;
            allSucceeded &= userResult.Success;

            var productResult = await ExecuteKanbanQueryAsync(
                "ProductCount",
                token => _context.Products.CountAsync(c => c.Tenant.Id == tid && !c.Deleted, token),
                fallback.ProductCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.ProductCount = productResult.Value;
            allSucceeded &= productResult.Success;

            var rulesResult = await ExecuteKanbanQueryAsync(
                "RulesCount",
                token => _context.FlowRules.CountAsync(c => c.Tenant.Id == tid && c.RuleStatus > -1, token),
                fallback.RulesCount,
                tid,
                cancellationToken,
                logger).ConfigureAwait(false);
            m.RulesCount = rulesResult.Value;
            allSucceeded &= rulesResult.Success;

            await _caching.SetAsync(cacheKey, m, KanbanCacheDuration, cancellationToken).ConfigureAwait(false);
            if (allSucceeded)
            {
                await _caching.SetAsync(staleCacheKey, m, KanbanStaleCacheDuration, cancellationToken).ConfigureAwait(false);
            }
            return m;
        }

        private static async Task<(T Value, bool Success)> ExecuteKanbanQueryAsync<T>(
            string metric,
            Func<CancellationToken, Task<T>> query,
            T fallback,
            Guid tenantId,
            CancellationToken cancellationToken,
            ILogger logger)
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(KanbanQueryTimeout);
            try
            {
                return (await query(timeoutSource.Token).ConfigureAwait(false), true);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger?.LogWarning("刷新看板缓存统计超时，使用上一次缓存值。TenantId={TenantId}, Metric={Metric}, TimeoutSeconds={TimeoutSeconds}", tenantId, metric, KanbanQueryTimeout.TotalSeconds);
                return (fallback, false);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "刷新看板缓存统计失败，使用上一次缓存值。TenantId={TenantId}, Metric={Metric}", tenantId, metric);
                return (fallback, false);
            }
        }
    }
}
