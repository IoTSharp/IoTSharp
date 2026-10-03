#nullable enable

using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 用独立 EF 上下文验证回执并发控制，避免终态和最新进度被旧快照覆盖。
/// </summary>
[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class EdgeTaskConcurrencyTests
{
    private readonly SqliteAppFixture _fixture;

    /// <summary>
    /// 复用 SQLite 宿主，测试对象仍以独立任务标识隔离。
    /// </summary>
    /// <param name="fixture">共享应用宿主。</param>
    public EdgeTaskConcurrencyTests(SqliteAppFixture fixture) => _fixture = fixture;

    /// <summary>
    /// 先保存成功终态，再保存旧 Running 快照时必须拒绝并回滚附带回执。
    /// </summary>
    [Fact]
    public async Task StaleRunningReceipt_CannotOverwriteTerminalStateOrPersistPartialReceipt()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        await AssertStaleReceiptRejectedAsync(EdgeTaskStatus.Succeeded, 100, cancellation.Token);
    }

    /// <summary>
    /// 状态仍为 Running 时，新回执时间也必须阻止旧上下文覆盖已经保存的新进度。
    /// </summary>
    [Fact]
    public async Task StaleRunningReceipt_CannotOverwriteNewerRunningProgressOrPersistPartialReceipt()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        await AssertStaleReceiptRejectedAsync(EdgeTaskStatus.Running, 80, cancellation.Token);
    }

    /// <summary>
    /// 状态和报告时间都未变化时，内部并发版本仍能阻止旧快照覆盖新进度。
    /// </summary>
    [Fact]
    public async Task SameTimestampRunningReceipts_RejectStaleProgressByStateRevision()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        await AssertStaleReceiptRejectedAsync(EdgeTaskStatus.Running, 80, cancellation.Token, sameTimestamp: true);
    }

    /// <summary>
    /// 两个独立上下文先读取同一版本，再顺序写入以确定性重现旧快照冲突。
    /// </summary>
    /// <param name="winningStatus">先提交的合法任务状态。</param>
    /// <param name="winningProgress">先提交的进度。</param>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    /// <param name="sameTimestamp">是否保持初始时间不变，以单独核验状态并发版本。</param>
    private async Task AssertStaleReceiptRejectedAsync(EdgeTaskStatus winningStatus, int winningProgress, CancellationToken cancellationToken, bool sameTimestamp = false)
    {
        var taskId = await SeedRunningTaskAsync(cancellationToken);
        using var winnerScope = _fixture.Services.CreateScope();
        using var staleScope = _fixture.Services.CreateScope();
        var winner = winnerScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stale = staleScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.NotSame(winner, stale);
        var winnerTask = await winner.EdgeTasks.SingleAsync(item => item.Id == taskId, cancellationToken);
        var staleTask = await stale.EdgeTasks.SingleAsync(item => item.Id == taskId, cancellationToken);
        Assert.Equal(EdgeTaskStatus.Running, winnerTask.Status);
        Assert.Equal(winnerTask.Status, staleTask.Status);
        Assert.Equal(winnerTask.LastReceiptAt, staleTask.LastReceiptAt);
        var initialReportedAt = Assert.IsType<DateTime>(winnerTask.LastReceiptAt);
        var winningReportedAt = sameTimestamp ? initialReportedAt : initialReportedAt.AddSeconds(2);
        var staleReportedAt = sameTimestamp ? initialReportedAt : initialReportedAt.AddSeconds(1);
        var winningReceiptId = Guid.NewGuid();
        var staleReceiptId = Guid.NewGuid();

        StageReceipt(winner, winnerTask, winningReceiptId, winningStatus, winningProgress, winningReportedAt);
        await winner.SaveChangesAsync(cancellationToken);

        StageReceipt(stale, staleTask, staleReceiptId, EdgeTaskStatus.Running, 25, staleReportedAt);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync(cancellationToken));

        using var verification = _fixture.Services.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await db.EdgeTasks.AsNoTracking().SingleAsync(item => item.Id == taskId, cancellationToken);
        Assert.Equal(winningStatus, persisted.Status);
        Assert.Equal(winningProgress, persisted.Progress);
        Assert.Equal(1, persisted.StateRevision);
        Assert.Equal(winningReportedAt, persisted.LastReceiptAt);
        Assert.Equal(winningReportedAt, persisted.UpdatedAt);
        Assert.Equal(winningStatus == EdgeTaskStatus.Succeeded ? winningReportedAt : (DateTime?)null, persisted.CompletedAt);
        var lastReceipt = JsonSerializer.Deserialize<EdgeTaskReceiptDto>(persisted.LastReceiptPayload);
        Assert.NotNull(lastReceipt);
        Assert.Equal(winningStatus, lastReceipt.Status);
        Assert.Equal(winningReportedAt, lastReceipt.ReportedAt);
        Assert.Equal(winningProgress, lastReceipt.Progress);
        var receipts = await db.EdgeTaskReceipts.AsNoTracking().Where(item => item.TaskId == taskId).ToListAsync(cancellationToken);
        var storedReceipt = Assert.Single(receipts);
        Assert.Equal(winningReceiptId, storedReceipt.Id);
        Assert.Equal(winningStatus, storedReceipt.Status);
        Assert.Equal(winningReportedAt, storedReceipt.ReportedAt);
        Assert.False(await db.EdgeTaskReceipts.AsNoTracking().AnyAsync(item => item.Id == staleReceiptId, cancellationToken));
    }

    /// <summary>
    /// 仅创建本测试使用的 Gateway 与 Running 任务，不调用真实执行端。
    /// </summary>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    /// <returns>已持久化的独立任务标识。</returns>
    private async Task<Guid> SeedRunningTaskAsync(CancellationToken cancellationToken)
    {
        using HttpClient client = _fixture.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        var created = await _fixture.CreateDeviceAsync(client, $"receipt-concurrency-{Guid.NewGuid():N}", DeviceType.Gateway);
        Assert.Equal((int)ApiCode.Success, created.Code);
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var device = await context.Device.AsNoTracking().SingleAsync(item => item.Id == created.Data!.Id, cancellationToken);
        var initialReportedAt = DateTime.UtcNow.AddSeconds(-10);
        var task = new EdgeTask
        {
            Id = Guid.NewGuid(),
            TaskType = EdgeTaskType.HealthProbe,
            TargetType = EdgeTaskTargetType.EdgeNode,
            GatewayId = device.Id,
            TargetKey = device.Id.ToString("D"),
            RuntimeType = EdgeRuntimeTypes.Gateway,
            InstanceId = string.Empty,
            Status = EdgeTaskStatus.Running,
            Progress = 10,
            Parameters = "{}",
            Metadata = "{}",
            RequestPayload = "{}",
            LastReceiptPayload = "{}",
            CreatedAt = initialReportedAt,
            UpdatedAt = initialReportedAt,
            LastReceiptAt = initialReportedAt,
            StartedAt = initialReportedAt,
            TenantId = device.TenantId,
            CustomerId = device.CustomerId
        };
        context.EdgeTasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return task.Id;
    }

    /// <summary>
    /// 在一次 SaveChanges 中暂存任务状态与附带历史，用来核验冲突时的原子回滚。
    /// </summary>
    /// <param name="context">独立写入上下文。</param>
    /// <param name="task">由该上下文读取的任务快照。</param>
    /// <param name="receiptId">独立回执标识。</param>
    /// <param name="status">上报状态。</param>
    /// <param name="progress">上报进度。</param>
    /// <param name="reportedAt">显式且互不相同的报告时间。</param>
    private static void StageReceipt(ApplicationDbContext context, EdgeTask task, Guid receiptId, EdgeTaskStatus status, int progress, DateTime reportedAt)
    {
        var receipt = new EdgeTaskReceiptDto
        {
            TaskId = task.Id,
            TargetType = task.TargetType,
            TargetKey = task.TargetKey,
            RuntimeType = task.RuntimeType,
            InstanceId = task.InstanceId,
            Status = status,
            Progress = progress,
            ReportedAt = reportedAt
        };
        var payload = JsonSerializer.Serialize(receipt);
        task.StateRevision = checked(task.StateRevision + 1);
        task.Status = status;
        task.Progress = progress;
        task.LastReceiptAt = reportedAt;
        task.LastReceiptPayload = payload;
        task.UpdatedAt = reportedAt;
        task.CompletedAt = status == EdgeTaskStatus.Succeeded ? reportedAt : null;
        context.EdgeTaskReceipts.Add(new EdgeTaskReceipt
        {
            Id = receiptId,
            TaskId = task.Id,
            GatewayId = task.GatewayId,
            TargetType = task.TargetType,
            TargetKey = task.TargetKey,
            RuntimeType = task.RuntimeType,
            InstanceId = task.InstanceId,
            Status = status,
            Progress = progress,
            Message = string.Empty,
            Result = "{}",
            Metadata = "{}",
            Payload = payload,
            ReportedAt = reportedAt,
            ReceivedAt = reportedAt,
            TenantId = task.TenantId,
            CustomerId = task.CustomerId
        });
    }
}
