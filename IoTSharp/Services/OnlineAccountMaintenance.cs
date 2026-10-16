#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Services;

/// <summary>供受保护的本机命令维护演示账号，不暴露额外 HTTP 管理入口。</summary>
internal sealed class OnlineAccountMaintenance(ApplicationDbContext context, UserManager<IdentityUser> users)
{
    internal const string DemoName = "demo@iotsharp.online";
    internal const string AdminName = "admin@iotsharp.online";
    private static readonly string[] DemoRoles = [nameof(UserRole.NormalUser), nameof(UserRole.CustomerAdmin), nameof(UserRole.TenantAdmin)];

    /// <summary>从标准输入接收一次性秘密，输出不含密码或密码哈希的执行回执。</summary>
    internal static async Task<int> RunCommandAsync(IServiceProvider services)
    {
        try
        {
            var input = new char[65537];
            var count = await Console.In.ReadBlockAsync(input, 0, input.Length);
            if (count == input.Length) throw new InvalidOperationException("维护请求超过长度限制。");
            var request = JsonSerializer.Deserialize<Request>(new string(input, 0, count))
                ?? throw new InvalidOperationException("维护请求不能为空。");
            Array.Clear(input);
            var context = services.GetRequiredService<ApplicationDbContext>();
            var maintenance = new OnlineAccountMaintenance(context, services.GetRequiredService<UserManager<IdentityUser>>());
            await using var transaction = await context.Database.BeginTransactionAsync();
            var receipt = await maintenance.ExecuteAsync(request);
            await transaction.CommitAsync();
            Console.WriteLine(JsonSerializer.Serialize(receipt));
            return 0;
        }
        catch (Exception exception)
        {
            // 维护输入包含秘密；只输出异常类型，不序列化输入或数据库参数。
            Console.Error.WriteLine("Account maintenance failed: " + exception.GetType().Name);
            return 1;
        }
    }

    /// <summary>首次配置管理员和独立演示租户，或按固定 ID 仅恢复演示账号。</summary>
    internal async Task<Receipt> ExecuteAsync(Request request)
    {
        if (request.Mode is not ("bootstrap" or "reset-demo"))
            throw new InvalidOperationException("不支持的账号维护模式。");
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DemoPassword);
        if (request.DemoPassword.Length < 6) throw new InvalidOperationException("演示密码过短。");

        IdentityUser? admin = null;
        IdentityUser demo;
        if (request.Mode == "bootstrap")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.AdminPassword);
            if (request.AdminPassword.Length < 6) throw new InvalidOperationException("管理员密码过短。");
            admin = await users.FindByNameAsync(AdminName) ?? await users.FindByNameAsync("demo@iotsharp.net")
                ?? throw new InvalidOperationException("已核实的初始管理员不存在。");
            if (!await users.IsInRoleAsync(admin, nameof(UserRole.SystemAdmin)))
                throw new InvalidOperationException("初始账号不是系统管理员。");

            demo = await users.FindByNameAsync(DemoName) ?? await CreateDemoAsync(request.DemoPassword);
            await RequireDemoScopeAsync(demo, null);
            await SetCredentialsAsync(admin, AdminName, request.AdminPassword);
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.DemoUserId);
            demo = await users.FindByIdAsync(request.DemoUserId)
                ?? throw new InvalidOperationException("已绑定的演示账号不存在；拒绝按邮箱重建或接管其它账号。");
            if (!Guid.TryParse(request.DemoTenantId, out var tenantId))
                throw new InvalidOperationException("演示租户绑定无效。");
            await RequireDemoScopeAsync(demo, tenantId);
        }

        await SetCredentialsAsync(demo, DemoName, request.DemoPassword);
        var claims = await users.GetClaimsAsync(demo);
        return new Receipt(request.Mode, demo.Id, claims.Single(c => c.Type == IoTSharpClaimTypes.Tenant).Value, admin?.Id);
    }

    /// <summary>创建专有演示租户，演示账号只有该租户内的管理角色。</summary>
    private async Task<IdentityUser> CreateDemoAsync(string password)
    {
        if (await context.Tenant.AnyAsync(t => t.Email == DemoName)
            || await context.Customer.AnyAsync(c => c.Email == DemoName))
            throw new InvalidOperationException("演示业务范围已存在但没有绑定账号，需先核对现状。");

        var tenant = new Tenant { Name = "IoTSharp Online Demo", Email = DemoName };
        var customer = new Customer { Name = "IoTSharp Online Demo", Email = DemoName, Tenant = tenant };
        tenant.Customers = [customer];
        context.Tenant.Add(tenant);
        var user = new IdentityUser { UserName = DemoName, Email = DemoName, EmailConfirmed = true };
        Ensure(await users.CreateAsync(user, password));
        Ensure(await users.AddClaimsAsync(user,
        [
            new Claim(ClaimTypes.Email, DemoName),
            new Claim(IoTSharpClaimTypes.Tenant, tenant.Id.ToString()),
            new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString())
        ]));
        Ensure(await users.AddToRolesAsync(user, DemoRoles));
        context.Relationship.Add(new Relationship { Id = Guid.NewGuid(), IdentityUser = user, Tenant = tenant, Customer = customer });
        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>校验固定身份和独立租户，拒绝把管理员或其它租户用户当成演示用户。</summary>
    private async Task RequireDemoScopeAsync(IdentityUser demo, Guid? expectedTenantId)
    {
        if (await users.IsInRoleAsync(demo, nameof(UserRole.SystemAdmin)))
            throw new InvalidOperationException("演示账号不能是系统管理员。");
        var claims = await users.GetClaimsAsync(demo);
        var tenantClaims = claims.Where(c => c.Type == IoTSharpClaimTypes.Tenant).ToArray();
        if (tenantClaims.Length != 1 || !Guid.TryParse(tenantClaims[0].Value, out var tenantId)
            || expectedTenantId.HasValue && tenantId != expectedTenantId.Value)
            throw new InvalidOperationException("演示租户身份不匹配。");
        if (!await context.Tenant.AnyAsync(t => t.Id == tenantId && t.Email == DemoName && !t.Deleted))
            throw new InvalidOperationException("演示账号不属于专有演示租户。");
    }

    /// <summary>由本机运维显式设置指定账号凭据，不更改公共注册的密码强度规则。</summary>
    private async Task SetCredentialsAsync(IdentityUser user, string name, string password)
    {
        user.UserName = name;
        user.Email = name;
        user.EmailConfirmed = true;
        // 管理员密码由运维明确指定；该例外仅存在于受保护的本机命令。
        user.PasswordHash = users.PasswordHasher.HashPassword(user, password);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        Ensure(await users.UpdateAsync(user));
        var emailClaims = (await users.GetClaimsAsync(user)).Where(c => c.Type == ClaimTypes.Email && c.Value != name).ToArray();
        if (emailClaims.Length != 0) Ensure(await users.RemoveClaimsAsync(user, emailClaims));
        if (!(await users.GetClaimsAsync(user)).Any(c => c.Type == ClaimTypes.Email && c.Value == name))
            Ensure(await users.AddClaimAsync(user, new Claim(ClaimTypes.Email, name)));
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("身份维护失败：" + string.Join(',', result.Errors.Select(e => e.Code)));
    }

    internal sealed class Request
    {
        public string Mode { get; set; } = "";
        public string DemoPassword { get; set; } = "";
        public string? AdminPassword { get; set; }
        public string? DemoUserId { get; set; }
        public string? DemoTenantId { get; set; }
    }

    internal sealed record Receipt(string Mode, string DemoUserId, string DemoTenantId, string? AdminUserId);
}
