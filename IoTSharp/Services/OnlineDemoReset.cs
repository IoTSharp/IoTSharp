#nullable enable
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storage.Net.Blobs;

namespace IoTSharp.Services;

internal sealed class OnlineDemoReset(OnlineDemoSettings settings, ApplicationDbContext db, UserManager<IdentityUser> users,
    SonnetDBStorage telemetry, IBlobStorage blobs)
{
    internal sealed record Journal(Guid TenantId, DateTime Hour, Guid Batch, DateTime CreatedAt, Guid[] OldDevices, string[] OldBlobs, bool Complete);
    internal sealed record Receipt(string Status, Guid TenantId, DateTime Hour, Guid Batch, int OldDevices);

    /// <summary>供本机定时服务执行一次有界恢复，状态文件用于断点恢复和同一小时去重。</summary>
    internal static async Task<int> RunCommandAsync(IServiceProvider services)
    {
        try
        {
            var settings = services.GetRequiredService<OnlineDemoSettings>();
            if (settings.AccountBinding is null)
            {
                Console.WriteLine("""{"Status":"disabled"}""");
                return 0;
            }
            if (services.GetRequiredService<IStorage>() is not SonnetDBStorage telemetry)
                throw new InvalidOperationException("在线演示重置需要 SonnetDB 时序存储。");

            var reset = new OnlineDemoReset(settings, services.GetRequiredService<ApplicationDbContext>(),
                services.GetRequiredService<UserManager<IdentityUser>>(), telemetry, services.GetRequiredService<IBlobStorage>());
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            var receipt = await reset.RunAsync(DateTime.UtcNow, timeout.Token);
            Console.WriteLine(JsonSerializer.Serialize(receipt));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("Demo reset failed: " + error.GetType().Name);
            return 1;
        }
    }

    /// <summary>先准备新批次，提交关系数据之后再回收旧的设备遥测和对象。</summary>
    internal async Task<Receipt> RunAsync(DateTime now, CancellationToken token)
    {
        var binding = settings.AccountBinding ?? throw new InvalidOperationException("演示维护未启用。");
        var tenantId = binding.DemoTenantId;
        var hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        // 部署时创建此目录并将写权限限定给运行账号。独占文件句柄串行化后台与人工重试。
        using var gate = new FileStream(Path.Combine(settings.StateDirectory, "reset.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var statePath = Path.Combine(settings.StateDirectory, "reset.json");
        var state = ReadJournal(statePath);
        if (state is not null && (state.TenantId != tenantId || state.Batch == Guid.Empty || state.Hour > hour
            || state.OldDevices.Length > 4096 || state.OldDevices.Contains(Guid.Empty)
            || state.OldBlobs.Length > 1000 || state.OldBlobs.Any(path => !IsTenantPath(path, tenantId))))
            throw new InvalidOperationException("演示重置日志与部署绑定不符。");

        var accounts = new OnlineDemoAccounts(db, users, binding);
        await using (var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token))
        {
            var scope = await accounts.VerifyAsync(token);
            if (state is { Complete: true } && state.Hour == hour)
                return new("current", tenantId, state.Hour, state.Batch, state.OldDevices.Length);

            if (state is null || state.Complete)
            {
                var old = await OnlineDemoCleanup.KeysAsync(db.Device.Where(device => device.TenantId == tenantId).Select(device => device.Id), token);
                var objects = await blobs.ListAsync(new ListOptions { FolderPath = $"/{tenantId:D}/", MaxResults = 1001, Recurse = true }, token);
                var paths = objects.Where(blob => !blob.IsFolder).Select(blob => blob.FullPath).ToArray();
                if (paths.Length > 1000 || paths.Any(path => !IsTenantPath(path, tenantId)))
                    throw new InvalidOperationException("演示对象超出已验证的范围。");

                state = new(tenantId, hour, Guid.NewGuid(), now, old, paths, false);
                SaveJournal(statePath, state);
            }

            var marker = OnlineDemoSeed.DeviceId(state.Batch, 0, 0);
            var committed = await db.Device.AnyAsync(d => d.TenantId == tenantId && d.Id == marker, token);
            if (!committed)
            {
                // 只有本事务读取的设备集合与日志相同，才可以在提交后回收旧测量。
                var current = await OnlineDemoCleanup.KeysAsync(db.Device.Where(device => device.TenantId == tenantId).Select(device => device.Id), token);
                if (!current.Order().SequenceEqual(state.OldDevices.Order()))
                    throw new InvalidOperationException("演示设备在未完成恢复期间发生变化。");

                await OnlineDemoCleanup.RemoveBusinessAsync(db, tenantId, current, token);
                var sites = await accounts.RestoreAsync(scope, settings.ReadPassword(), token);
                var keep = sites.Select(site => site.Customer.Id).Append(scope.PrimaryCustomer.Id).ToHashSet();
                db.Customer.RemoveRange(scope.Customers.Where(customer => !keep.Contains(customer.Id)));
                for (var index = 0; index < sites.Count; index++)
                    OnlineDemoSeed.AddSite(db, scope.Tenant, sites[index], state.Batch, index, state.CreatedAt);
                await db.SaveChangesAsync(token);
                token.ThrowIfCancellationRequested();
                var report = await telemetry.StoreTelemetryBatchAsync(OnlineDemoSeed.History(state.Batch, state.CreatedAt));
                if (!report.IsComplete || report.WrittenRows != 540 || report.SkippedTelemetryValueCount != 0)
                    throw new InvalidOperationException("演示历史遥测写入不完整。");
                await transaction.CommitAsync(token);
            }
        }

        // 在重试或提交结果未知的情况下重新核验，避免删除已经归属其它租户的全局 ID。
        if (state.OldDevices.Length != 0
            && await db.Device.AnyAsync(d => state.OldDevices.Contains(d.Id), token))
            throw new InvalidOperationException("待回收测量仍有设备记录。");
        foreach (var device in state.OldDevices)
        {
            token.ThrowIfCancellationRequested();
            await telemetry.RemoveDeviceHistoryAsync(device, token);
        }
        await blobs.DeleteAsync(state.OldBlobs, token);
        SaveJournal(statePath, state with { Complete = true });
        return new("reset", tenantId, state.Hour, state.Batch, state.OldDevices.Length);
    }

    private static bool IsTenantPath(string path, Guid tenantId) =>
        path.TrimStart('/').StartsWith($"{tenantId:D}/", StringComparison.Ordinal) && !path.Contains('\\')
        && !path.Split('/').Any(segment => segment is "." or "..");

    private static Journal? ReadJournal(string path) => File.Exists(path)
        ? JsonSerializer.Deserialize<Journal>(File.ReadAllBytes(path)) ?? throw new InvalidOperationException("演示重置日志无效。")
        : null;

    private static void SaveJournal(string path, Journal value)
    {
        var temporary = path + ".next";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, value);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporary, path, overwrite: true);
    }
}
