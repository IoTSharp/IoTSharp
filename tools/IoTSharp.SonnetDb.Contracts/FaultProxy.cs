using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTSharp.SonnetDb.Contracts;

/// <summary>仅监听回环地址的有界测试代理，注入会话缺失及提交响应丢失，不记录凭据或 SQL 正文。</summary>
internal sealed class FaultProxy : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client = new(new SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };
    private int _dropped;
    private int _requests;
    internal ConcurrentQueue<WireEvent> Events { get; } = new();
    internal bool RejectSessionCreation { get; set; }
    internal bool LoseCommitResponse { get; set; }
    internal bool HideTransactionState { get; set; }
    internal bool HoldNextSqlResponse { get; set; }
    internal bool AbortNextSqlResponse { get; set; }
    internal TaskCompletionSource SqlResponseReached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal Func<Task>? AfterCommit { get; set; }
    internal string BaseUrl { get; private set; } = "";

    /// <summary>创建只用于当前验收进程的代理；调用方负责异步释放。</summary>
    private FaultProxy(string upstream)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [] });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        _app = builder.Build();
        _app.Run(context => ForwardAsync(context, upstream));
    }

    /// <summary>启动代理并读取系统分配的回环端口。</summary>
    internal static async Task<FaultProxy> StartAsync(string upstream, CancellationToken token)
    {
        var proxy = new FaultProxy(upstream);
        try
        {
            await proxy._app.StartAsync(token);
            proxy.BaseUrl = proxy._app.Services.GetRequiredService<IServer>().Features
                .Get<IServerAddressesFeature>()!.Addresses.Single();
            return proxy;
        }
        catch
        {
            await proxy.DisposeAsync();
            throw;
        }
    }

    /// <summary>转发真实服务器请求，在服务器确认提交后才丢弃客户端响应。</summary>
    private async Task ForwardAsync(HttpContext context, string upstream)
    {
        var path = context.Request.Path.Value ?? "/";
        if (Interlocked.Increment(ref _requests) > 1000)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }
        var isSession = path.Contains("/sql/transactions", StringComparison.Ordinal);
        var isCreation = isSession && path.EndsWith("/transactions", StringComparison.Ordinal)
            && context.Request.Method == "POST";
        if ((RejectSessionCreation && isCreation) || (HideTransactionState && isSession && context.Request.Method == "GET"))
        {
            Events.Enqueue(new(context.Request.Method, path, 404, false));
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        deadline.CancelAfter(TimeSpan.FromSeconds(25));
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method),
            upstream + path + context.Request.QueryString);
        if (context.Request.ContentLength is > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
            request.Content = new ByteArrayContent(await ReadBodyAsync(context.Request.Body, deadline.Token));
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)) continue;
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        try
        {
            using var response = await _client.SendAsync(request, deadline.Token);
            var body = await response.Content.ReadAsByteArrayAsync(deadline.Token);
            var commit = isSession && path.EndsWith("/commit", StringComparison.Ordinal)
                && context.Request.Method == "POST";
            var drop = commit && response.IsSuccessStatusCode && LoseCommitResponse
                && Interlocked.CompareExchange(ref _dropped, 1, 0) == 0;
            var sql = isSession && path.EndsWith("/sql", StringComparison.Ordinal) && response.IsSuccessStatusCode;
            if (sql && (HoldNextSqlResponse || AbortNextSqlResponse))
            {
                var hold = HoldNextSqlResponse;
                HoldNextSqlResponse = false;
                AbortNextSqlResponse = false;
                Events.Enqueue(new(context.Request.Method, path, (int)response.StatusCode, true));
                SqlResponseReached.TrySetResult();
                if (hold) await Task.Delay(TimeSpan.FromSeconds(10), deadline.Token);
                context.Abort();
                return;
            }
            Events.Enqueue(new(context.Request.Method, path, (int)response.StatusCode, drop));
            if (drop)
            {
                if (AfterCommit != null) await AfterCommit();
                context.Abort();
                return;
            }
            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString();
            foreach (var header in response.Headers.Where(item => item.Key.StartsWith("X-SonnetDB-", StringComparison.OrdinalIgnoreCase)))
                context.Response.Headers[header.Key] = header.Value.ToArray();
            await context.Response.Body.WriteAsync(body, deadline.Token);
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or IOException)
        {
            Events.Enqueue(new(context.Request.Method, path, 502, false));
            context.Abort();
        }
    }

    /// <summary>限制请求正文为 1 MiB，避免测试代理无限缓存。</summary>
    private static async Task<byte[]> ReadBodyAsync(Stream stream, CancellationToken token)
    {
        using var content = new MemoryStream();
        var buffer = new byte[8192];
        for (var chunk = 0; chunk < 129; chunk++)
        {
            var read = await stream.ReadAsync(buffer, token);
            if (read == 0) return content.ToArray();
            if (content.Length + read > 1024 * 1024) throw new IOException("Proxy body limit exceeded");
            await content.WriteAsync(buffer.AsMemory(0, read), token);
        }
        throw new IOException("Proxy body chunk limit exceeded");
    }

    /// <summary>停止并释放本进程创建的代理监听器。</summary>
    public async ValueTask DisposeAsync()
    {
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try { await _app.StopAsync(stop.Token); }
        finally { await _app.DisposeAsync(); _client.Dispose(); }
    }
}

internal sealed record WireEvent(string Method, string Path, int Status, bool ResponseDropped);
