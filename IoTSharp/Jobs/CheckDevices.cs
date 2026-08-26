using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Jobs
{
    [QuartzJobScheduler(60)]
    [DisallowConcurrentExecution]
    public sealed class CheckDevices : IJob
    {
        private readonly ILogger<CheckDevices> _logger;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IPublisher _queue;

        public CheckDevices(
            ILogger<CheckDevices> logger,
            IServiceScopeFactory scopeFactor,
            IPublisher queue)
        {
            _logger = logger;
            _scopeFactor = scopeFactor;
            _queue = queue;
        }

        /// <summary>
        /// 检查超时设备并把仍处于活跃状态的设备转换为非活跃状态。
        /// </summary>
        /// <param name="context">Quartz 作业上下文。</param>
        public Task Execute(IJobExecutionContext context)
            => CheckInactiveDevicesAsync(DateTime.UtcNow, context.CancellationToken);

        /// <summary>
        /// 执行一次设备离线状态检查，只有条件更新成功后才发布离线事件。
        /// </summary>
        /// <param name="utcNow">本轮检查采用的 UTC 时间。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>成功从活跃状态转换为非活跃状态的设备数量。</returns>
        internal async Task<int> CheckInactiveDevicesAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactor.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // DataStorage 自连接会放大候选扫描；两个属性查询都可命中
                // (Catalog, KeyName, DeviceId) 前缀，再按 DeviceId 在内存汇合。
                var activeStates = await dbContext.AttributeLatest
                    .AsNoTracking()
                    .Where(item => item.DataSide == DataSide.ServerSide
                        && item.KeyName == Constants._Active
                        && item.Value_Boolean == true)
                    .Select(item => new
                    {
                        item.DeviceId,
                        ActiveStateUpdatedUtc = item.DateTime
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (activeStates.Count == 0)
                {
                    return 0;
                }

                var devicesById = await dbContext.Device
                    .AsNoTracking()
                    .Where(device => !device.Deleted)
                    .Select(device => new
                    {
                        device.Id,
                        device.Name,
                        device.Timeout
                    })
                    .ToDictionaryAsync(device => device.Id, cancellationToken)
                    .ConfigureAwait(false);

                var lastActivityByDevice = await dbContext.AttributeLatest
                    .AsNoTracking()
                    .Where(item => item.DataSide == DataSide.ServerSide
                        && item.KeyName == Constants._LastActivityDateTime)
                    .Select(item => new
                    {
                        item.DeviceId,
                        LastActivityUtc = item.Value_DateTime
                    })
                    .ToDictionaryAsync(item => item.DeviceId, cancellationToken)
                    .ConfigureAwait(false);

                var transitionedCount = 0;
                foreach (var activeState in activeStates)
                {
                    if (!devicesById.TryGetValue(activeState.DeviceId, out var device)
                        || !lastActivityByDevice.TryGetValue(activeState.DeviceId, out var lastActivity)
                        || !lastActivity.LastActivityUtc.HasValue
                        || utcNow.Subtract(lastActivity.LastActivityUtc.Value).TotalSeconds <= device.Timeout)
                    {
                        continue;
                    }

                    var changed = await dbContext.AttributeLatest
                        .Where(item => item.DeviceId == activeState.DeviceId
                            && item.DataSide == DataSide.ServerSide
                            && item.KeyName == Constants._Active
                            && item.Value_Boolean == true
                            && item.DateTime == activeState.ActiveStateUpdatedUtc)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(item => item.Value_Boolean, false)
                                .SetProperty(item => item.DateTime, utcNow),
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (changed == 0)
                    {
                        continue;
                    }

                    try
                    {
                        await _queue.PublishActive(activeState.DeviceId, ActivityStatus.Inactivity).ConfigureAwait(false);
                    }
                    catch
                    {
                        await dbContext.AttributeLatest
                            .Where(item => item.DeviceId == activeState.DeviceId
                                && item.DataSide == DataSide.ServerSide
                                && item.KeyName == Constants._Active
                                && item.Value_Boolean == false
                                && item.DateTime == utcNow)
                            .ExecuteUpdateAsync(
                                setters => setters
                                    .SetProperty(item => item.Value_Boolean, true)
                                    .SetProperty(item => item.DateTime, activeState.ActiveStateUpdatedUtc),
                                cancellationToken)
                            .ConfigureAwait(false);
                        throw;
                    }

                    transitionedCount++;
                    _logger.LogDebug(
                        "设备 {DeviceName} ({DeviceId}) 已转换为非活跃状态，上次活跃时间 {LastActivityUtc}，超时 {TimeoutSeconds} 秒。",
                        device.Name,
                        activeState.DeviceId,
                        lastActivity.LastActivityUtc,
                        device.Timeout);
                }

                if (transitionedCount > 0)
                {
                    _logger.LogInformation(
                        "设备在线状态检查完成，共将 {TransitionedCount} 台超时设备转换为非活跃状态。",
                        transitionedCount);
                }

                return transitionedCount;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查设备在线状态错误。");
                return 0;
            }
        }
    }
}
