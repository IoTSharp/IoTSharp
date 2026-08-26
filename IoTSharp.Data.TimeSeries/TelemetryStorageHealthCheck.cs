using IoTSharp.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Data.TimeSeries;

/// <summary>
/// 检查当前遥测存储是否可读。
/// </summary>
internal sealed class TelemetryStorageHealthCheck : IHealthCheck
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);
    private readonly IStorage _storage;

    /// <summary>
    /// 创建遥测存储健康检查。
    /// </summary>
    /// <param name="storage">当前配置的遥测存储。</param>
    public TelemetryStorageHealthCheck(IStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// 在限定时间内执行遥测存储探针。
    /// </summary>
    /// <param name="context">健康检查上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>遥测存储连接状态。</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthy = await _storage.CheckTelemetryStorage()
                .WaitAsync(Timeout, cancellationToken)
                .ConfigureAwait(false);
            return healthy
                ? HealthCheckResult.Healthy("遥测存储可用。")
                : HealthCheckResult.Unhealthy("遥测存储不可用。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("遥测存储连接检查失败。", ex);
        }
    }
}
