using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using SonnetDB.Data;

namespace IoTSharp.SonnetDb.Contracts;

/// <summary>拥有单个临时容器；有界调用 Docker 并核对标签后回收，不操作其它容器或共享卷。</summary>
internal sealed class DockerRuntime(string docker, string image, string output) : IAsyncDisposable
{
    private readonly string _owner = Guid.NewGuid().ToString("N");
    private readonly List<object> _processes = [];
    private string? _containerId;
    internal string BaseUrl { get; private set; } = "";
    internal string FrameBaseUrl { get; private set; } = "";
    internal string Database { get; } = "contract_" + Guid.NewGuid().ToString("N");
    internal const string Token = "iotsharp_isolated_contract_test_only";
    internal bool CleanupComplete { get; private set; }

    /// <summary>使用仅回环映射的临时容器，不拉取或重新发布镜像。</summary>
    internal async Task StartAsync(CancellationToken token)
    {
        var inspect = await RunAsync(["image", "inspect", image], TimeSpan.FromSeconds(15), token);
        await File.WriteAllTextAsync(Path.Combine(output, "image.json"), inspect, token);
        using var reservation = new TcpListener(IPAddress.Loopback, 0);
        reservation.Start(1);
        var port = ((IPEndPoint)reservation.LocalEndpoint).Port;
        using var frameReservation = new TcpListener(IPAddress.Loopback, 0);
        frameReservation.Start(1);
        var framePort = ((IPEndPoint)frameReservation.LocalEndpoint).Port;
        frameReservation.Stop();
        reservation.Stop();
        var id = await RunAsync(["run", "--detach", "--pull", "never", "--name", "iotsharp-contract-" + _owner,
            "--label", "iotsharp.contract.owner=" + _owner, "--cpus", "1", "--memory", "1g",
            "--publish", $"127.0.0.1:{port}:5080", "--env", "ASPNETCORE_URLS=http://+:5080",
            "--publish", $"127.0.0.1:{framePort}:5081",
            "--env", "SONNETDB_SonnetDBServer__DataRoot=/data",
            "--env", "SONNETDB_SonnetDBServer__AutoLoadExistingDatabases=true",
            "--env", "SONNETDB_SonnetDBServer__Tokens__" + Token + "=admin", image], TimeSpan.FromSeconds(30), token);
        _containerId = id.Trim();
        if (_containerId.Length != 64 || !_containerId.All(char.IsAsciiHexDigit))
            throw new InvalidOperationException("Docker did not return an exact container ID");
        await File.WriteAllTextAsync(Path.Combine(output, "container.json"),
            JsonSerializer.Serialize(new { id = _containerId, owner = _owner }), token);
        var binding = (await RunAsync(["port", _containerId, "5080/tcp"], TimeSpan.FromSeconds(10), token)).Trim();
        if (!binding.StartsWith("127.0.0.1:", StringComparison.Ordinal) || binding.Contains('\n'))
            throw new InvalidOperationException("Expected one loopback port binding");
        BaseUrl = "http://" + binding;
        FrameBaseUrl = $"http://127.0.0.1:{framePort}";
        await WaitForDatabaseAsync(token);
    }

    /// <summary>构造仅指向本任务数据库的连接串。</summary>
    internal string ConnectionString(string? endpoint = null, string protocol = "rest")
        => $"Data Source=sonnetdb+{endpoint ?? BaseUrl}/{Database};Token={Token};Timeout=15;Protocol={protocol}";

    /// <summary>只重启当前容器，保留其可写层以检验服务重启对账。</summary>
    internal async Task RestartAsync(CancellationToken token)
    {
        await VerifyOwnerAsync(token);
        await RunAsync(["restart", "--time", "2", _containerId!], TimeSpan.FromSeconds(30), token);
        var binding = (await RunAsync(["port", _containerId!, "5080/tcp"], TimeSpan.FromSeconds(10), token)).Trim();
        if (BaseUrl != "http://" + binding) throw new InvalidOperationException("Container port changed across restart");
        await WaitForDatabaseAsync(token);
    }

    /// <summary>最多等待 40 次、每次间隔 500ms；单请求及总验收令牌共同限制启动时间。</summary>
    private async Task WaitForDatabaseAsync(CancellationToken token)
    {
        using var startup = CancellationTokenSource.CreateLinkedTokenSource(token);
        startup.CancelAfter(TimeSpan.FromSeconds(45));
        Exception? last = null;
        for (var attempt = 0; attempt < 40; attempt++)
        {
            startup.Token.ThrowIfCancellationRequested();
            try
            {
                SndbResourceInitializer.EnsureDatabase(ConnectionString(), "isolated contract consumer");
                await using var connection = new SndbConnection(ConnectionString());
                await connection.OpenAsync(startup.Token);
                await using var command = connection.CreateCommand();
                command.CommandText = "SHOW TABLES";
                await using var reader = await command.ExecuteReaderAsync(startup.Token);
                return;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                last = error;
                await Task.Delay(500, startup.Token);
            }
        }
        throw new InvalidOperationException("SonnetDB did not become ready within 40 attempts", last);
    }

    /// <summary>同时核对容器 ID 和本次唯一标签，避免按名字或镜像批量操作。</summary>
    private async Task VerifyOwnerAsync(CancellationToken token)
    {
        if (_containerId == null) throw new InvalidOperationException("No owned container");
        var label = await RunAsync(["inspect", "--format", "{{index .Config.Labels \"iotsharp.contract.owner\"}}", _containerId],
            TimeSpan.FromSeconds(10), token);
        if (label.Trim() != _owner) throw new InvalidOperationException("Container ownership check failed");
    }

    /// <summary>字面 argv、墙钟超时、进程身份记录和仅本任务子树回收。</summary>
    private async Task<string> RunAsync(string[] args, TimeSpan timeout, CancellationToken token)
    {
        var start = new ProcessStartInfo(docker)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Docker did not start");
        _processes.Add(new
        {
            pid = process.Id,
            parentPid = Environment.ProcessId,
            startedAtUtc = process.StartTime.ToUniversalTime(),
            program = docker,
            args,
            timeoutSeconds = timeout.TotalSeconds
        });
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(timeout);
        var stdout = process.StandardOutput.ReadToEndAsync(deadline.Token);
        var stderr = process.StandardError.ReadToEndAsync(deadline.Token);
        try
        {
            await process.WaitForExitAsync(deadline.Token);
            var result = await stdout;
            var errors = await stderr;
            if (process.ExitCode != 0) throw new InvalidOperationException($"Docker {args[0]} exit {process.ExitCode}: {errors}");
            if (result.Length > 256 * 1024) throw new InvalidOperationException("Docker output limit exceeded");
            return result;
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await process.WaitForExitAsync(cleanup.Token);
            }
        }
    }

    /// <summary>保留测试服务日志，验证标签后删除当前容器及其可写层；不删除镜像。</summary>
    public async ValueTask DisposeAsync()
    {
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            if (_containerId == null)
            {
                var recovered = await RunAsync(["ps", "--all", "--quiet", "--no-trunc", "--filter", "label=iotsharp.contract.owner=" + _owner],
                    TimeSpan.FromSeconds(10), cleanup.Token);
                if (!string.IsNullOrWhiteSpace(recovered))
                {
                    _containerId = recovered.Trim();
                    if (_containerId.Length != 64 || !_containerId.All(char.IsAsciiHexDigit))
                        throw new InvalidOperationException("Unknown container creation outcome requires manual ownership reconciliation");
                }
            }
            if (_containerId != null)
            {
                await VerifyOwnerAsync(cleanup.Token);
                try
                {
                    var logs = await RunAsync(["logs", "--tail", "150", _containerId], TimeSpan.FromSeconds(10), cleanup.Token);
                    await File.WriteAllTextAsync(Path.Combine(output, "server.log"), logs, cleanup.Token);
                }
                finally { await RunAsync(["rm", "--force", _containerId], TimeSpan.FromSeconds(15), cleanup.Token); }
            }
            CleanupComplete = true;
        }
        finally
        {
            await File.WriteAllTextAsync(Path.Combine(output, "processes.json"), JsonSerializer.Serialize(_processes), CancellationToken.None);
        }
    }
}
