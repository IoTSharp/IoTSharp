#nullable enable
using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Services;

internal static class OnlinePersonalTenant
{
    internal sealed record Request(string UserId, Guid TenantId, Guid CustomerId, bool Apply = false);

    /// <summary>核对固定管理员的独立范围，可将其租户与客户资料改为自用名称。</summary>
    internal static async Task<int> RunAsync(IServiceProvider services)
    {
        try
        {
            var buffer = new char[4096];
            var count = await Console.In.ReadBlockAsync(buffer, 0, buffer.Length);
            if (count == buffer.Length) throw new InvalidOperationException("请求超过长度限制。");
            var request = JsonSerializer.Deserialize<Request>(buffer.AsSpan(0, count).ToString())
                ?? throw new InvalidOperationException("缺少管理员归属。");
            var db = services.GetRequiredService<ApplicationDbContext>();
            var users = services.GetRequiredService<UserManager<IdentityUser>>();
            var receipt = await ExecuteAsync(db, users, request);
            Console.WriteLine(JsonSerializer.Serialize(receipt));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("Personal tenant maintenance failed: " + error.GetType().Name);
            return 1;
        }
    }

    internal static async Task<object> ExecuteAsync(ApplicationDbContext db, UserManager<IdentityUser> users, Request request)
    {
        const string email = "mysticboy@live.com";
        const string name = "IoTSharp Personal";
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var account = await users.FindByIdAsync(request.UserId);
        if (account is null || account.UserName != email || !await users.IsInRoleAsync(account, nameof(UserRole.SystemAdmin)))
            throw new InvalidOperationException("管理员身份与固定绑定不符。");

        var claims = await users.GetClaimsAsync(account);
        if (!HasScope(claims, IoTSharpClaimTypes.Tenant, request.TenantId)
            || !HasScope(claims, IoTSharpClaimTypes.Customer, request.CustomerId))
            throw new InvalidOperationException("管理员的声明与固定绑定不符。");

        var tenant = await db.Tenant.SingleAsync(t => t.Id == request.TenantId && !t.Deleted);
        var customer = await db.Customer.SingleAsync(c => c.Id == request.CustomerId && !c.Deleted
            && EF.Property<Guid?>(c, "TenantId") == tenant.Id);
        var relationships = await db.Relationship.Where(r => EF.Property<Guid?>(r, "TenantId") == tenant.Id
            || EF.Property<string>(r, "IdentityUserId") == account.Id).ToListAsync();
        var relationship = relationships.Single();
        if (db.Entry(relationship).Property<Guid?>("CustomerId").CurrentValue != customer.Id
            || db.Entry(relationship).Property<Guid?>("TenantId").CurrentValue != tenant.Id
            || db.Entry(relationship).Property<string>("IdentityUserId").CurrentValue != account.Id
            || await db.Customer.CountAsync(c => EF.Property<Guid?>(c, "TenantId") == tenant.Id) != 1)
            throw new InvalidOperationException("指定租户不是管理员独立使用的范围。");

        var devices = await db.Device.CountAsync(d => d.TenantId == tenant.Id && !d.Deleted);
        if (await db.Device.AnyAsync(d => d.TenantId == tenant.Id && !d.Deleted && d.CustomerId != customer.Id))
            throw new InvalidOperationException("存在设备归属差异。");

        if (request.Apply)
        {
            if (await db.Tenant.AnyAsync(t => t.Id != tenant.Id && t.Email == email)
                || await db.Customer.AnyAsync(c => c.Id != customer.Id && c.Email == email))
                throw new InvalidOperationException("联系邮箱已用于其它业务范围。");
            tenant.Name = customer.Name = name;
            tenant.Email = customer.Email = email;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return new { UserId = account.Id, TenantId = tenant.Id, CustomerId = customer.Id, tenant.Name, DeviceCount = devices, request.Apply };
    }

    internal static bool HasScope(System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> claims, string type, Guid id)
    {
        var values = claims.Where(claim => claim.Type == type).Take(2).ToArray();
        return id != Guid.Empty && values.Length == 1 && Guid.TryParse(values[0].Value, out var value) && value == id;
    }
}
