#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 验证回执来源由服务端认证路径确定，且管理操作不会在审计中署名为运行时。
/// </summary>
[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class ReceiptProvenanceTests
{
    private const string SourceKey = "receiptSource";
    private const string ActorKey = "receiptActorId";
    private const string RuntimeSource = "RuntimeAuthenticated";
    private const string ManagementSource = "ManagementAuthenticated";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SqliteAppFixture _fixture;

    /// <summary>
    /// 复用 SQLite 应用宿主，避免与其他集成测试并发创建共享设施。
    /// </summary>
    /// <param name="fixture">共享应用宿主。</param>
    public ReceiptProvenanceTests(SqliteAppFixture fixture) => _fixture = fixture;

    /// <summary>
    /// 管理回执保留既有状态入口，同时覆盖客户端伪造来源并记录真实用户。
    /// </summary>
    [Fact]
    public async Task ManagementReceipt_OverridesForgedSourceAndAuditsAuthenticatedUser()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var management = CreateClient();
        var login = await _fixture.LoginAsync(management);
        var task = await PrepareRunningTaskAsync(management, cancellation.Token);
        string userId;
        using (var scope = _fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            userId = await context.Users.AsNoTracking().Where(user => user.Email == login.Data!.UserName)
                .Select(user => user.Id).SingleAsync(cancellation.Token);
        }

        using var response = await management.PostAsJsonAsync("/api/EdgeTask/Receipt", CreateTerminalReceipt(task.Request, RuntimeSource), cancellation.Token);
        var receipt = await ReadApiResultAsync<EdgeTaskReceiptDto>(response, cancellation.Token);
        Assert.Equal((int)ApiCode.Success, receipt.Code);
        AssertProvenance(receipt.Data!.Metadata, ManagementSource, userId);
        await AssertStoredProvenanceAsync(task.Request.TaskId, ManagementSource, userId, cancellation.Token);

        using var verify = _fixture.Services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var audit = await db.AuditLog.AsNoTracking().SingleAsync(log => log.ObjectID == task.Request.TaskId
            && log.ObjectType == ObjectType.EdgeTask && log.ActionName == "EdgeTaskManagementReceipt", cancellation.Token);
        Assert.Equal(userId, audit.UserId);
        Assert.DoesNotContain("edge-runtime:", audit.UserName, StringComparison.OrdinalIgnoreCase);
        Assert.False(await db.AuditLog.AsNoTracking().AnyAsync(log => log.ObjectID == task.Request.TaskId
            && log.ActionName == "EdgeTaskTerminalReceipt", cancellation.Token));
    }

    /// <summary>
    /// 匿名运行时必须以通道令牌上报，持久化来源不能被载荷伪装成管理回执。
    /// </summary>
    [Fact]
    public async Task RuntimeReceipt_OverridesForgedSourceAndPersistsChannelIdentity()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var management = CreateClient();
        var task = await PrepareRunningTaskAsync(management, cancellation.Token);
        using var runtime = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/EdgeTask/Receipt")
        {
            Content = JsonContent.Create(CreateTerminalReceipt(task.Request, ManagementSource))
        };
        request.Headers.Add("X-Edge-Access-Token", task.AccessToken);
        using var response = await runtime.SendAsync(request, cancellation.Token);
        var receipt = await ReadApiResultAsync<EdgeTaskReceiptDto>(response, cancellation.Token);
        Assert.Equal((int)ApiCode.Success, receipt.Code);
        var actorId = task.GatewayId.ToString("D");
        AssertProvenance(receipt.Data!.Metadata, RuntimeSource, actorId);
        await AssertStoredProvenanceAsync(task.Request.TaskId, RuntimeSource, actorId, cancellation.Token);

        using var verify = _fixture.Services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var audit = await db.AuditLog.AsNoTracking().SingleAsync(log => log.ObjectID == task.Request.TaskId
            && log.ObjectType == ObjectType.EdgeTask && log.ActionName == "EdgeTaskTerminalReceipt", cancellation.Token);
        Assert.Equal(actorId, audit.UserId);
        Assert.StartsWith("edge-runtime:", audit.UserName);
        Assert.False(await db.AuditLog.AsNoTracking().AnyAsync(log => log.ObjectID == task.Request.TaskId
            && log.ActionName == "EdgeTaskManagementReceipt", cancellation.Token));
    }

    /// <summary>
    /// Accept 的路径令牌决定来源；同时携带管理端 JWT 也不能改变运行时归属。
    /// </summary>
    [Fact]
    public async Task AcceptDispatch_UsesRuntimeSourceAndOverridesCaseVariantKeys()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var management = CreateClient();
        var task = await CreateAndPullTaskAsync(management, cancellation.Token);
        using var response = await management.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{task.AccessToken}/Accept", new EdgeTaskReceiptDto
        {
            TaskId = task.Request.TaskId,
            ReportedAt = DateTime.UtcNow,
            Metadata = ForgedMetadata(ManagementSource)
        }, cancellation.Token);
        var result = await ReadApiResultAsync<object>(response, cancellation.Token);
        Assert.Equal((int)ApiCode.Success, result.Code);
        await AssertStoredProvenanceAsync(task.Request.TaskId, RuntimeSource, task.GatewayId.ToString("D"), cancellation.Token);
    }

    /// <summary>
    /// 持有正确通道令牌也不能以伪造寻址推进正式任务或污染回执历史。
    /// </summary>
    /// <param name="field">本次伪造的寻址字段；每个输入对应一项独立负例。</param>
    [Theory]
    [InlineData("targetKey")]
    [InlineData("runtimeType")]
    [InlineData("instanceId")]
    public async Task AcceptDispatch_RejectsForgedAddressWithoutChangingTaskOrReceipts(string field)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var management = CreateClient();
        var task = await CreateAndPullTaskAsync(management, cancellation.Token);
        EdgeTask before;
        int receiptCount;
        int auditCount;
        using (var baseline = _fixture.Services.CreateScope())
        {
            var context = baseline.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            before = await context.EdgeTasks.AsNoTracking().SingleAsync(item => item.Id == task.Request.TaskId, cancellation.Token);
            receiptCount = await context.EdgeTaskReceipts.AsNoTracking().CountAsync(item => item.TaskId == task.Request.TaskId, cancellation.Token);
            auditCount = await context.AuditLog.AsNoTracking().CountAsync(item => item.ObjectID == task.Request.TaskId
                && item.ObjectType == ObjectType.EdgeTask, cancellation.Token);
        }
        Assert.Equal(EdgeTaskStatus.Sent, before.Status);
        var forged = new EdgeTaskReceiptDto
        {
            TaskId = task.Request.TaskId,
            TargetKey = field == "targetKey" ? $"{task.GatewayId:D}:forged-target" : task.Request.Address.TargetKey,
            RuntimeType = field == "runtimeType" ? "forged-runtime" : task.Request.Address.RuntimeType,
            InstanceId = field == "instanceId" ? "forged-instance" : task.Request.Address.InstanceId,
            ReportedAt = DateTime.UtcNow
        };
        using var response = await management.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{task.AccessToken}/Accept", forged, cancellation.Token);
        var result = await ReadApiResultAsync<object>(response, cancellation.Token);
        Assert.Equal((int)ApiCode.InValidData, result.Code);
        Assert.Contains("Acceptance address", result.Msg, StringComparison.OrdinalIgnoreCase);

        using var verification = _fixture.Services.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var after = await db.EdgeTasks.AsNoTracking().SingleAsync(item => item.Id == task.Request.TaskId, cancellation.Token);
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.TargetKey, after.TargetKey);
        Assert.Equal(before.RuntimeType, after.RuntimeType);
        Assert.Equal(before.InstanceId, after.InstanceId);
        Assert.Equal(before.AcceptedAt, after.AcceptedAt);
        Assert.Equal(before.LastReceiptAt, after.LastReceiptAt);
        Assert.Equal(before.LastReceiptPayload, after.LastReceiptPayload);
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
        Assert.Equal(receiptCount, await db.EdgeTaskReceipts.AsNoTracking().CountAsync(item => item.TaskId == task.Request.TaskId, cancellation.Token));
        Assert.Equal(auditCount, await db.AuditLog.AsNoTracking().CountAsync(item => item.ObjectID == task.Request.TaskId
            && item.ObjectType == ObjectType.EdgeTask, cancellation.Token));
    }

    /// <summary>
    /// 创建带有限 HTTP 超时的独立客户端。
    /// </summary>
    /// <returns>调用方负责释放的应用客户端。</returns>
    private HttpClient CreateClient()
    {
        var client = _fixture.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    /// <summary>
    /// 通过正式管理和运行时入口创建处于 Running 的诊断任务。
    /// </summary>
    /// <param name="management">管理客户端。</param>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    /// <returns>任务寻址及其通道凭据。</returns>
    private async Task<PreparedTask> PrepareRunningTaskAsync(HttpClient management, CancellationToken cancellationToken)
    {
        var task = await CreateAndPullTaskAsync(management, cancellationToken);
        using var accepted = await management.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{task.AccessToken}/Accept", new EdgeTaskReceiptDto
        {
            TaskId = task.Request.TaskId,
            ReportedAt = DateTime.UtcNow
        }, cancellationToken);
        Assert.Equal((int)ApiCode.Success, (await ReadApiResultAsync<object>(accepted, cancellationToken)).Code);
        using var runningRequest = new HttpRequestMessage(HttpMethod.Post, "/api/EdgeTask/Receipt")
        {
            Content = JsonContent.Create(new EdgeTaskReceiptDto
            {
                TaskId = task.Request.TaskId,
                TargetKey = task.Request.Address.TargetKey,
                RuntimeType = task.Request.Address.RuntimeType,
                Status = EdgeTaskStatus.Running,
                Progress = 10,
                ReportedAt = DateTime.UtcNow
            })
        };
        runningRequest.Headers.Add("X-Edge-Access-Token", task.AccessToken);
        using var running = await management.SendAsync(runningRequest, cancellationToken);
        Assert.Equal((int)ApiCode.Success, (await ReadApiResultAsync<EdgeTaskReceiptDto>(running, cancellationToken)).Code);
        return task;
    }

    /// <summary>
    /// 为每个测试创建独立 Gateway 与正式任务，再使用通道令牌将任务置为 Sent。
    /// </summary>
    /// <param name="management">已授权或可由 fixture 授权的管理客户端。</param>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    /// <returns>独立任务的寻址及通道凭据。</returns>
    private async Task<PreparedTask> CreateAndPullTaskAsync(HttpClient management, CancellationToken cancellationToken)
    {
        var device = await _fixture.CreateDeviceAsync(management, $"receipt-provenance-{Guid.NewGuid():N}", DeviceType.Gateway);
        var gatewayId = device.Data!.Id;
        var token = await _fixture.GetDeviceAccessTokenAsync(management, gatewayId);
        var request = new EdgeTaskRequestDto
        {
            TaskId = Guid.NewGuid(),
            TaskType = EdgeTaskType.HealthProbe,
            CreatedAt = DateTime.UtcNow,
            Address = new EdgeTaskAddressDto
            {
                DeviceId = gatewayId,
                TargetType = EdgeTaskTargetType.EdgeNode,
                TargetKey = gatewayId.ToString("D"),
                RuntimeType = EdgeRuntimeTypes.Gateway
            },
            Parameters = new Dictionary<string, object> { ["probe"] = "ping" }
        };
        using var dispatched = await management.PostAsJsonAsync("/api/EdgeTask/Dispatch", request, cancellationToken);
        Assert.Equal((int)ApiCode.Success, (await ReadApiResultAsync<EdgeTaskRequestDto>(dispatched, cancellationToken)).Code);
        using var pulled = await management.GetAsync($"/api/EdgeTask/Dispatch/{token}", cancellationToken);
        var tasks = await ReadApiResultAsync<List<EdgeTaskRequestDto>>(pulled, cancellationToken);
        Assert.Equal((int)ApiCode.Success, tasks.Code);
        Assert.Contains(tasks.Data!, item => item.TaskId == request.TaskId);
        return new PreparedTask(request, gatewayId, token);
    }

    /// <summary>
    /// 构造带大小写变体来源伪造的合法终态载荷，确保服务端处理所有保留键。
    /// </summary>
    /// <param name="task">正式任务寻址。</param>
    /// <param name="forgedSource">客户端声明的相反来源。</param>
    /// <returns>可由来源测试提交的终态回执。</returns>
    private static EdgeTaskReceiptDto CreateTerminalReceipt(EdgeTaskRequestDto task, string forgedSource) => new()
    {
        TaskId = task.TaskId,
        TargetKey = task.Address.TargetKey,
        RuntimeType = task.Address.RuntimeType,
        Status = EdgeTaskStatus.Succeeded,
        Progress = 100,
        ReportedAt = DateTime.UtcNow,
        Metadata = ForgedMetadata(forgedSource)
    };

    /// <summary>
    /// 生成同一保留键的多种大小写以及普通业务元数据。
    /// </summary>
    /// <param name="source">伪造的来源。</param>
    /// <returns>未规范化的客户端元数据。</returns>
    private static Dictionary<string, string> ForgedMetadata(string source) => new()
    {
        [SourceKey] = source,
        ["ReceiptSource"] = "ClientControlled",
        ["RECEIPTSOURCE"] = "ClientControlledUpper",
        [ActorKey] = "forged-actor",
        ["ReceiptActorId"] = "forged-actor-title",
        ["RECEIPTACTORID"] = "forged-actor-upper",
        ["testMarker"] = "preserved"
    };

    /// <summary>
    /// 验证历史实体、原始载荷和任务最近回执都保留同一服务端认证来源。
    /// </summary>
    /// <param name="taskId">独立测试任务。</param>
    /// <param name="source">预期服务端来源。</param>
    /// <param name="actorId">预期认证主体。</param>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    private async Task AssertStoredProvenanceAsync(Guid taskId, string source, string actorId, CancellationToken cancellationToken)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var receipt = await context.EdgeTaskReceipts.AsNoTracking().Where(item => item.TaskId == taskId)
            .OrderByDescending(item => item.ReceivedAt).FirstAsync(cancellationToken);
        AssertProvenance(JsonSerializer.Deserialize<Dictionary<string, string>>(receipt.Metadata, JsonOptions)!, source, actorId);
        AssertProvenance(JsonSerializer.Deserialize<EdgeTaskReceiptDto>(receipt.Payload, JsonOptions)!.Metadata, source, actorId);
        var task = await context.EdgeTasks.AsNoTracking().SingleAsync(item => item.Id == taskId, cancellationToken);
        AssertProvenance(JsonSerializer.Deserialize<EdgeTaskReceiptDto>(task.LastReceiptPayload, JsonOptions)!.Metadata, source, actorId);
    }

    /// <summary>
    /// 保留键必须只有服务端标准大小写，普通元数据保持原值。
    /// </summary>
    /// <param name="metadata">反序列化的回执元数据。</param>
    /// <param name="source">预期来源。</param>
    /// <param name="actorId">预期主体。</param>
    private static void AssertProvenance(IReadOnlyDictionary<string, string> metadata, string source, string actorId)
    {
        Assert.Equal(source, metadata[SourceKey]);
        Assert.Equal(actorId, metadata[ActorKey]);
        Assert.Equal(SourceKey, Assert.Single(metadata.Keys.Where(key => string.Equals(key, SourceKey, StringComparison.OrdinalIgnoreCase))));
        Assert.Equal(ActorKey, Assert.Single(metadata.Keys.Where(key => string.Equals(key, ActorKey, StringComparison.OrdinalIgnoreCase))));
        Assert.Equal("preserved", metadata["testMarker"]);
    }

    /// <summary>
    /// 读取保持 HTTP 与 API 成功语义分离的响应，避免只凭 HTTP 200 判定成功。
    /// </summary>
    /// <typeparam name="T">API 返回的数据类型。</typeparam>
    /// <param name="response">待释放由调用方管理的响应。</param>
    /// <param name="cancellationToken">测试墙钟取消信号。</param>
    /// <returns>可断言业务状态的结果。</returns>
    private static async Task<ApiResult<T>> ReadApiResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResult<T>>(cancellationToken);
        Assert.NotNull(result);
        return result;
    }

    private sealed record PreparedTask(EdgeTaskRequestDto Request, Guid GatewayId, string AccessToken);
}
