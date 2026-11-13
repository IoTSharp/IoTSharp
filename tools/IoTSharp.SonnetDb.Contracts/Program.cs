using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SonnetDB.Data;

namespace IoTSharp.SonnetDb.Contracts;

/// <summary>正式 5.0.0 包的隔离消费验收，所有断言仅针对本程序创建的临时数据库。</summary>
internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>运行固定数量的合同用例，保留 PASS/FAIL/NOT_RUN 和机器可读回执；Ctrl+C 可取消。</summary>
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        if (args.Length is < 2 or > 7 || args[0] != "--output")
        {
            Console.Error.WriteLine("Usage: --output <new-absolute-directory> [--smoke] [--image <existing-image>] [--docker <absolute-exe>]");
            return 2;
        }
        var output = Path.GetFullPath(args[1]);
        if (!Path.IsPathFullyQualified(args[1]) || Directory.Exists(output))
            throw new ArgumentException("Output must be a new absolute directory");
        var docker = Option("--docker") ?? @"C:\Program Files\Docker\Docker\resources\bin\docker.exe";
        var image = Option("--image") ?? "iotsharp/sonnetdb:5.0.0";
        var smoke = args.Contains("--smoke", StringComparer.Ordinal);
        Directory.CreateDirectory(output);
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(6));
        ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; deadline.Cancel(); };
        Console.CancelKeyPress += cancel;
        var cases = new List<CaseResult>();
        var runtime = new DockerRuntime(docker, image, output);
        var suite = new ContractSuite(runtime, cases);
        try
        {
            await runtime.StartAsync(deadline.Token);
            await using (var setup = new ContractDbContext(runtime.ConnectionString()))
                await setup.Database.EnsureCreatedAsync(deadline.Token);
            await suite.RunAsync("conditional-update", suite.ConditionalUpdateAsync, deadline.Token);
            if (!smoke) await suite.RunAllAsync(deadline.Token);
        }
        catch (Exception error)
        {
            cases.Add(new("initialization-or-deadline", "FAIL", [Describe(error)], []));
            Console.Error.WriteLine(Describe(error));
        }
        finally
        {
            try { await runtime.DisposeAsync(); }
            catch (Exception error) { cases.Add(new("resource-cleanup", "FAIL", [Describe(error)], [])); }
            Console.CancelKeyPress -= cancel;
            var packages = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                assembly.GetName().Name?.StartsWith("SonnetDB", StringComparison.Ordinal) == true
                || assembly == typeof(DbContext).Assembly).OrderBy(assembly => assembly.GetName().Name);
            var provenance = packages.Select(assembly => new
            {
                name = assembly.GetName().Name,
                version = assembly.GetName().Version?.ToString(),
                informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                path = assembly.Location,
                sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(assembly.Location)))
            }).ToArray();
            await File.WriteAllTextAsync(Path.Combine(output, "results.json"), JsonSerializer.Serialize(new
            {
                observedAtUtc = DateTime.UtcNow,
                runtime = Environment.Version.ToString(),
                smoke,
                scope = "isolated contract slice; not IoTSharp HTTP/business acceptance or production enablement",
                image,
                provenance,
                cleanupComplete = runtime.CleanupComplete,
                cases,
                failClosedProviderRequired = true
            }, JsonOptions));
            Console.WriteLine($"Result: {cases.Count(item => item.Status == "PASS")} PASS, {cases.Count(item => item.Status == "FAIL")} FAIL, {cases.Count(item => item.Status == "NOT_RUN")} NOT_RUN; {output}");
        }
        return cases.All(item => item.Status == "PASS") ? 0 : 1;

        string? Option(string key)
        {
            var index = Array.IndexOf(args, key);
            return index < 0 ? null : index + 1 < args.Length ? args[index + 1] : throw new ArgumentException(key);
        }
    }

    /// <summary>保留结构化数据库错误码和原异常类型，避免把并发冲突、取消与提交未知混为一类。</summary>
    internal static string Describe(Exception error)
        => $"{error.GetType().FullName}: {error.Message}"
           + (error is SndbServerException server ? $" [serverError={server.Error}]" : "")
           + (error.InnerException != null ? $" -> {Describe(error.InnerException)}" : "");
}

internal sealed record CaseResult(string Name, string Status, IReadOnlyList<string> Evidence, IReadOnlyList<WireEvent> Wire);

/// <summary>每个场景拥有独立操作编号和 Gateway；新上下文回读数据库中真实存储的结果。</summary>
internal sealed class ContractSuite(DockerRuntime runtime, List<CaseResult> results)
{
    private readonly List<string> _evidence = [];
    private readonly List<WireEvent> _wire = [];

    /// <summary>有界执行固定用例表，前置取消使后续项目保持 NOT_RUN。</summary>
    internal async Task RunAllAsync(CancellationToken token)
    {
        (string Name, Func<CancellationToken, Task> Run)[] cases =
        [
            ("real-session-and-five-table-commit", CommitAsync),
            ("rollback-at-each-business-stage", RollbackStagesAsync),
            ("busy-expired-deleted-and-owner", LeasePredicatesAsync),
            ("multi-gateway-partial-acquisition-rollback", BatchRollbackAsync),
            ("read-committed-lease-competition", ct => CompetitionAsync(IsolationLevel.ReadCommitted, ct)),
            ("serializable-lease-competition", ct => CompetitionAsync(IsolationLevel.Serializable, ct)),
            ("cancel-before-commit", CancellationAsync),
            ("sql-response-timeout-rollback", ct => InterruptedReadAsync(timeout: true, ct)),
            ("sql-response-disconnect-rollback", ct => InterruptedReadAsync(timeout: false, ct)),
            ("lost-commit-response-reconciled", ct => LostCommitAsync(hideState: false, restart: false, ct)),
            ("lost-commit-response-unknown", ct => LostCommitAsync(hideState: true, restart: false, ct)),
            ("server-restart-uncommitted", RestartUncommittedAsync),
            ("server-restart-committed-unknown", ct => LostCommitAsync(hideState: false, restart: true, ct)),
            ("legacy-server-session-rejected", LegacyServerAsync),
            ("frame-connection-real-transaction", FrameAsync)
        ];
        foreach (var test in cases) await RunAsync(test.Name, test.Run, token);
    }

    /// <summary>单例执行上限 50 秒、总进程上限 6 分钟，失败不伪装为跳过。</summary>
    internal async Task RunAsync(string name, Func<CancellationToken, Task> action, CancellationToken token)
    {
        _evidence.Clear();
        _wire.Clear();
        if (token.IsCancellationRequested)
        {
            results.Add(new(name, "NOT_RUN", ["Overall deadline/cancellation reached"], []));
            return;
        }
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(TimeSpan.FromSeconds(50));
        var status = "PASS";
        try { await action(deadline.Token); }
        catch (Exception error) { status = "FAIL"; _evidence.Add(Program.Describe(error)); }
        results.Add(new(name, status, _evidence.ToArray(), _wire.ToArray()));
        Console.WriteLine($"{status} {name}: {string.Join("; ", _evidence.Take(4))}");
    }

    /// <summary>以 GUID、UTC ticks、布尔条件验证非事务条件更新、真实行数和重新查询。</summary>
    internal async Task ConditionalUpdateAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var context = Context();
        Require(await context.AcquireAsync(gateway, operation, token) == 1, "First update must affect exactly one row");
        Require(await context.AcquireAsync(gateway, Guid.NewGuid(), token) == 0, "Busy update must affect zero rows");
        await using var verify = Context();
        var row = await verify.Nodes.SingleAsync(item => item.GatewayId == gateway, token);
        Require(row.ConfigurationOperationId == operation && row.ConfigurationOperationExpiresAt > DateTime.UtcNow
            && row.ConfigurationOperationOwner == "contract-consumer", "Updated fields must survive fresh-context readback");
        Require(await context.ReleaseAsync(gateway, operation, token) == 1, "Owner release must affect one row");
        _evidence.Add("affected=[1,0,1]; nullable GUID, UTC DateTime2, boolean and multi-setter round-trip");
    }

    /// <summary>证明命令发送到真实事务 session，五类关系写入和条件释放在提交后同时可见。</summary>
    private async Task CommitAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var proxy = await FaultProxy.StartAsync(runtime.BaseUrl, token);
        try
        {
            await using var context = Context(proxy.BaseUrl);
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire must affect one row");
            await WriteAllAsync(context, operation, gateway, 1, token);
            await VerifyRowsAsync(operation, 0, token);
            Require(await context.ReleaseAsync(gateway, operation, token) == 1, "Release must affect one row");
            await transaction.CommitAsync(token);
            await VerifyRowsAsync(operation, 1, token);
            await VerifyLeaseAsync(gateway, null, token);
            Require(proxy.Events.Any(item => item.Method == "POST" && item.Path.EndsWith("/transactions", StringComparison.Ordinal) && item.Status == 200), "A real server session must be acknowledged");
            Require(proxy.Events.Any(item => item.Path.Contains("/transactions/", StringComparison.Ordinal) && item.Path.EndsWith("/sql", StringComparison.Ordinal)), "DML must use that server session");
            Require(!proxy.Events.Any(item => item.Path.Contains("preview", StringComparison.OrdinalIgnoreCase)), "Preflight replay cannot establish transaction ownership");
            _evidence.Add("five tables invisible before commit, all=[1,1,1,1,1] after commit; real session paths captured");
        }
        finally { _wire.AddRange(proxy.Events); }
    }

    /// <summary>在每个实际保存阶段注入失败，检查整个业务组合及租约都回滚。</summary>
    private async Task RollbackStagesAsync(CancellationToken token)
    {
        for (var failureStage = 0; failureStage < 5; failureStage++)
        {
            var gateway = await SeedAsync(token);
            var operation = Guid.NewGuid();
            try
            {
                await using var context = Context();
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
                Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire must affect one row");
                for (var stage = 0; stage <= failureStage; stage++)
                    await context.WriteStageAsync(stage, operation, gateway, 1, token);
                throw new InjectedStageFailure();
            }
            catch (InjectedStageFailure) { }
            await VerifyRowsAsync(operation, 0, token);
            await VerifyLeaseAsync(gateway, null, token);
            _evidence.Add($"failureStage={failureStage}; all business rows=0, lease=null");
        }
    }

    /// <summary>验证 busy、过期接管、删除标志与错误 owner 释放，不只检查 SQL 能否生成。</summary>
    private async Task LeasePredicatesAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var oldOperation = Guid.NewGuid();
        var operation = Guid.NewGuid();
        await using var context = Context();
        Require(await context.AcquireAsync(gateway, oldOperation, token) == 1, "Seed active lease");
        Require(await context.ReleaseAsync(gateway, operation, token) == 0, "Foreign owner must not release");
        await context.Nodes.Where(item => item.GatewayId == gateway).ExecuteUpdateAsync(setters => setters
            .SetProperty(item => item.ConfigurationOperationExpiresAt, DateTime.UtcNow.AddMinutes(-1)), token);
        Require(await context.AcquireAsync(gateway, operation, token) == 1, "Expired lease must be replaceable");
        Require(await context.ReleaseAsync(gateway, oldOperation, token) == 0, "Old owner must not release new lease");
        Require(await context.ReleaseAsync(gateway, operation, token) == 1, "New owner can release");
        await context.Nodes.Where(item => item.GatewayId == gateway).ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Deleted, true), token);
        Require(await context.AcquireAsync(gateway, Guid.NewGuid(), token) == 0, "Deleted node must not acquire");
        _evidence.Add("foreign/old owner=0; expired takeover=1; deleted target=0");
    }

    /// <summary>第二目标繁忙时回滚首个目标的租约和已保存业务记录。</summary>
    private async Task BatchRollbackAsync(CancellationToken token)
    {
        var first = await SeedAsync(token);
        var second = await SeedAsync(token);
        var operation = Guid.NewGuid();
        var busyOperation = Guid.NewGuid();
        await using (var busy = Context()) Require(await busy.AcquireAsync(second, busyOperation, token) == 1, "Busy seed");
        await using (var context = Context())
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            Require(await context.AcquireAsync(first, operation, token) == 1, "First target acquired");
            await WriteAllAsync(context, operation, first, 1, token);
            Require(await context.AcquireAsync(second, operation, token) == 0, "Second target must reject");
            await transaction.RollbackAsync(token);
        }
        await VerifyLeaseAsync(first, null, token);
        await VerifyLeaseAsync(second, busyOperation, token);
        await VerifyRowsAsync(operation, 0, token);
        _evidence.Add("affected=[1,0]; first lease and five-table writes rolled back, existing busy lease preserved");
    }

    /// <summary>两个独立上下文固定交错：都取得预提交影响数、写任务、释放租约，再依次提交检验 ABA 竞争。</summary>
    private async Task CompetitionAsync(IsolationLevel isolation, CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operations = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await using var first = Context();
        await using var second = Context();
        var contexts = new[] { first, second };
        var transactions = new IDbContextTransaction?[2];
        var committed = new bool[2];
        try
        {
            for (var i = 0; i < 2; i++)
            {
                transactions[i] = await contexts[i].Database.BeginTransactionAsync(isolation, token);
                var affected = await contexts[i].AcquireAsync(gateway, operations[i], token);
                _evidence.Add($"participant={i}; acquireAffected={affected}");
                Require(affected == 1, "Both transactions must reach the controlled precommit boundary");
                Require(!await contexts[i].Set<BusinessRow<EdgeTask>>().AnyAsync(item => item.GatewayId == gateway, token),
                    "Both participants must inspect in-flight work before either commit");
                await WriteAllAsync(contexts[i], operations[i], gateway, i + 1, token);
                Require(await contexts[i].ReleaseAsync(gateway, operations[i], token) == 1, "Own conditional release");
            }
            for (var i = 0; i < 2; i++)
            {
                try { await transactions[i]!.CommitAsync(token); committed[i] = true; }
                catch (Exception error) { _evidence.Add($"participant={i}; commitError={Program.Describe(error)}"); }
            }
            for (var i = 0; i < 2; i++) await VerifyRowsAsync(operations[i], committed[i] ? 1 : 0, token);
            _evidence.Add($"committed={string.Join(',', committed)}; isolation={isolation}");
            Require(committed.Count(item => item) == 1, "Exactly one competing operation may commit; affected=1 alone is not ownership");
            if (isolation == IsolationLevel.Serializable)
            {
                foreach (var transaction in transactions) if (transaction != null) await DisposeTransactionAsync(transaction);
                await using var retry = Context();
                await using var retryTransaction = await retry.Database.BeginTransactionAsync(isolation, token);
                Require(await retry.AcquireAsync(gateway, operations[1], token) == 1, "Fresh retry must reacquire in a new transaction");
                Require(await retry.Set<BusinessRow<EdgeTask>>().AnyAsync(item => item.GatewayId == gateway, token),
                    "Retry must observe the winner's in-flight task and reject duplicate dispatch");
                await retryTransaction.RollbackAsync(token);
                await VerifyRowsAsync(operations[1], 0, token);
                _evidence.Add("bounded whole-operation retry=1; re-read winner's in-flight task and rejected loser without partial writes");
            }
        }
        finally
        {
            foreach (var transaction in transactions)
                if (transaction != null) await DisposeTransactionAsync(transaction);
        }
    }

    /// <summary>已写入五阶段后取消提交，显式回滚并用未取消的新上下文对账。</summary>
    private async Task CancellationAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var proxy = await FaultProxy.StartAsync(runtime.BaseUrl, token);
        try
        {
            await using var context = Context(proxy.BaseUrl);
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire");
            await WriteAllAsync(context, operation, gateway, 1, token);
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            var error = await CaptureAsync(() => transaction.CommitAsync(cancelled.Token));
            _evidence.Add("commitResult=" + (error == null ? "unexpected success" : Program.Describe(error)));
            Require(error is OperationCanceledException || error is IOException && error.Message.Contains("未知", StringComparison.Ordinal),
                "Cancellation must not report confirmed commit success");
            Require(!proxy.Events.Any(item => item.Path.EndsWith("/commit", StringComparison.Ordinal)), "Pre-cancelled commit must not dispatch");
            var rollbackError = await CaptureAsync(() => transaction.RollbackAsync(token));
            await VerifyRowsAsync(operation, 0, token);
            await VerifyLeaseAsync(gateway, null, token);
            _evidence.Add("no commit request dispatched; fresh readback five rows=0 and lease=null");
            if (rollbackError != null)
            {
                _evidence.Add("providerRollback=" + Program.Describe(rollbackError));
                var sessionPath = proxy.Events.Last(item => item.Method == "GET" && item.Path.Contains("/transactions/", StringComparison.Ordinal)).Path;
                using var recovery = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(5) };
                recovery.DefaultRequestHeaders.Authorization = new("Bearer", DockerRuntime.Token);
                using var state = await recovery.GetAsync(runtime.BaseUrl + sessionPath, token);
                _evidence.Add("serverSession=" + await state.Content.ReadAsStringAsync(token));
                using var rollback = await recovery.PostAsync(runtime.BaseUrl + sessionPath + "/rollback", null, token);
                rollback.EnsureSuccessStatusCode();
                _evidence.Add("test-only session cleanup performed via the captured session ID; provider failure remains FAIL");
            }
            Require(rollbackError == null, "Cancelled commit completed the local transaction before its active server session could be rolled back");
        }
        finally { _wire.AddRange(proxy.Events); }
    }

    /// <summary>在服务器已执行事务读后注入超时或断连，确认请求失败不把未提交的五表状态变成部分提交。</summary>
    private async Task InterruptedReadAsync(bool timeout, CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var proxy = await FaultProxy.StartAsync(runtime.BaseUrl, token);
        try
        {
            await using var context = Context(proxy.BaseUrl);
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire");
            await WriteAllAsync(context, operation, gateway, 1, token);
            proxy.HoldNextSqlResponse = timeout;
            proxy.AbortNextSqlResponse = !timeout;
            using var queryDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
            queryDeadline.CancelAfter(TimeSpan.FromSeconds(2));
            var failed = CaptureAsync(async () => await context.Nodes.CountAsync(queryDeadline.Token));
            await proxy.SqlResponseReached.Task.WaitAsync(TimeSpan.FromSeconds(3), token);
            var error = await failed;
            Require(error != null, "Injected response interruption must be observed by the client");
            _evidence.Add("queryResult=" + Program.Describe(error!));
            await transaction.RollbackAsync(token);
            await VerifyRowsAsync(operation, 0, token);
            await VerifyLeaseAsync(gateway, null, token);
            _evidence.Add("fault applied after server response; rollback leaves five rows=0 and lease=null");
        }
        finally { _wire.AddRange(proxy.Events); }
    }

    /// <summary>服务器提交成功后丢失响应，分别检验终态回读、终态不可用与重启后的业务编号对账。</summary>
    private async Task LostCommitAsync(bool hideState, bool restart, CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var proxy = await FaultProxy.StartAsync(runtime.BaseUrl, token);
        proxy.LoseCommitResponse = true;
        proxy.HideTransactionState = hideState;
        if (restart) proxy.AfterCommit = () => runtime.RestartAsync(token);
        IDbContextTransaction? transaction = null;
        try
        {
            await using var context = Context(proxy.BaseUrl);
            transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire");
            await WriteAllAsync(context, operation, gateway, 1, token);
            Require(await context.ReleaseAsync(gateway, operation, token) == 1, "Release");
            var error = await CaptureAsync(() => transaction.CommitAsync(token));
            _evidence.Add($"operationId={operation}; commitResult={(error == null ? "confirmed" : Program.Describe(error))}");
            Require(proxy.Events.Count(item => item.ResponseDropped) == 1, "Fault must actually drop a successful commit response");
            Require(proxy.Events.Count(item => item.Method == "POST" && item.Path.EndsWith("/commit", StringComparison.Ordinal)) == 1, "Unknown commit must not blindly retry/replay writes");
            Require(proxy.Events.Any(item => item.Method == "GET" && item.Path.Contains("/transactions/", StringComparison.Ordinal)), "Client must query original session status");
            if (hideState || restart)
                Require(error is IOException && error.Message.Contains("未知", StringComparison.Ordinal), "Unavailable terminal state must remain commit-unknown");
            else Require(error == null, "Confirmed committed terminal state must reconcile successfully");
            await VerifyRowsAsync(operation, 1, token);
            await VerifyLeaseAsync(gateway, null, token);
            _evidence.Add("fresh direct connection reconciled all five committed records exactly once");
        }
        finally
        {
            if (transaction != null) await DisposeTransactionAsync(transaction);
            _wire.AddRange(proxy.Events);
        }
    }

    /// <summary>重启会丢失未提交会话，客户端不能把其缓存写入重放到新的事务。</summary>
    private async Task RestartUncommittedAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var context = Context();
        var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        try
        {
            Require(await context.AcquireAsync(gateway, operation, token) == 1, "Acquire");
            await WriteAllAsync(context, operation, gateway, 1, token);
            await runtime.RestartAsync(token);
            var error = await CaptureAsync(() => transaction.CommitAsync(token));
            Require(error is IOException && error.Message.Contains("未知", StringComparison.Ordinal), "Lost session commit must be unknown");
            await VerifyRowsAsync(operation, 0, token);
            await VerifyLeaseAsync(gateway, null, token);
            _evidence.Add($"operationId={operation}; {Program.Describe(error!)}; five rows absent after restart");
        }
        finally { await DisposeTransactionAsync(transaction); }
    }

    /// <summary>旧服务端不支持事务 session 时，Serializable 必须拒绝，不接受预演影响数。</summary>
    private async Task LegacyServerAsync(CancellationToken token)
    {
        await using var proxy = await FaultProxy.StartAsync(runtime.BaseUrl, token);
        proxy.RejectSessionCreation = true;
        try
        {
            await using var context = Context(proxy.BaseUrl);
            var error = await CaptureAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            });
            Require(error is NotSupportedException, "Legacy server must not silently downgrade Serializable");
            Require(!proxy.Events.Any(item => item.Path.EndsWith("/preview", StringComparison.Ordinal)), "No ownership inferred from preview");
            _evidence.Add(Program.Describe(error!));
        }
        finally { _wire.AddRange(proxy.Events); }
    }

    /// <summary>强制 Frame 连接的事务控制和事务内读写仍必须保持同一真实服务端会话。</summary>
    private async Task FrameAsync(CancellationToken token)
    {
        var gateway = await SeedAsync(token);
        var operation = Guid.NewGuid();
        await using var context = new ContractDbContext(runtime.ConnectionString(runtime.FrameBaseUrl, "frame-http2"));
        Require(await context.Nodes.AnyAsync(item => item.GatewayId == gateway, token), "Forced HTTP/2 Frame read must find seed");
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        Require(await context.Nodes.AnyAsync(item => item.GatewayId == gateway, token), "Transactional read must find seed");
        Require(await context.AcquireAsync(gateway, operation, token) == 1, "Frame connection acquire");
        await WriteAllAsync(context, operation, gateway, 1, token);
        await transaction.RollbackAsync(token);
        await VerifyRowsAsync(operation, 0, token);
        _evidence.Add("Protocol=frame-http2 on the image's HTTP/2 port 5081; ordinary Frame read and real-session transaction rollback passed");
        _evidence.Add("transaction control/DML use REST session routes over HTTP/2; port 5080 is the HTTP/1 endpoint");
    }

    /// <summary>建立一个独立 Gateway 锚点，所有读取均限定当前隔离测试数据库。</summary>
    private async Task<Guid> SeedAsync(CancellationToken token)
    {
        var gateway = Guid.NewGuid();
        await using var context = Context();
        context.Nodes.Add(new() { Id = gateway, GatewayId = gateway });
        await context.SaveChangesAsync(token);
        return gateway;
    }

    /// <summary>创建新的独立连接上下文，不复用已失败事务的跟踪状态。</summary>
    private ContractDbContext Context(string? endpoint = null) => new(runtime.ConnectionString(endpoint));

    /// <summary>每个阶段单独发送 SaveChanges，确保验证的不是只在内存中添加实体。</summary>
    private static async Task WriteAllAsync(ContractDbContext context, Guid operation, Guid gateway, long version, CancellationToken token)
    {
        for (var stage = 0; stage < 5; stage++) await context.WriteStageAsync(stage, operation, gateway, version, token);
    }

    /// <summary>精确比对各张业务表的操作记录数量。</summary>
    private async Task VerifyRowsAsync(Guid operation, int expected, CancellationToken token)
    {
        await using var verify = Context();
        var counts = await verify.ReadOperationAsync(operation, token);
        Require(counts.All(item => item == expected), $"operation={operation}; expected={expected}; actual=[{string.Join(',', counts)}]");
    }

    /// <summary>从新连接核对租约所有者，防止部分写入被异常掩盖。</summary>
    private async Task VerifyLeaseAsync(Guid gateway, Guid? expected, CancellationToken token)
    {
        await using var verify = Context();
        var lease = await verify.Nodes.Where(item => item.GatewayId == gateway).Select(item => item.ConfigurationOperationId).SingleAsync(token);
        Require(lease == expected, $"gateway={gateway}; expectedLease={expected}; actualLease={lease}");
    }

    /// <summary>保留已失败事务释放的附加错误，不让清理异常覆盖提交结果。</summary>
    private async Task DisposeTransactionAsync(IDbContextTransaction transaction)
    {
        try { await transaction.DisposeAsync(); }
        catch (Exception error) { _evidence.Add("transactionCleanup=" + Program.Describe(error)); }
    }

    /// <summary>捕获预期的失败结果，成功时返回空值。</summary>
    private static async Task<Exception?> CaptureAsync(Func<Task> action)
    {
        try { await action(); return null; }
        catch (Exception error) { return error; }
    }

    /// <summary>合同不满足时明确失败，不降级为能力通过。</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>只用于中断已保存阶段，验证异常展开时的事务释放和回滚。</summary>
    private sealed class InjectedStageFailure : Exception;
}
