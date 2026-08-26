using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.HealthChecks;

/// <summary>
/// 检查 IoTSharp 主数据库是否可连接。
/// </summary>
internal sealed class ApplicationDatabaseHealthCheck : IHealthCheck
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// 创建主数据库健康检查。
    /// </summary>
    /// <param name="dbContext">IoTSharp 主数据库上下文。</param>
    public ApplicationDatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 在限定时间内验证主数据库连接。
    /// </summary>
    /// <param name="context">健康检查上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>主数据库连接状态。</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);
            var canConnect = await _dbContext.Database.CanConnectAsync(timeout.Token).ConfigureAwait(false);
            return canConnect
                ? HealthCheckResult.Healthy("IoTSharp 主数据库可用。")
                : HealthCheckResult.Unhealthy("IoTSharp 主数据库不可用。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("IoTSharp 主数据库连接检查失败。", ex);
        }
    }
}
