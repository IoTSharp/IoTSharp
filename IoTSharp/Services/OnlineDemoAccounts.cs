#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IoTSharp.Services;

internal sealed class OnlineDemoAccounts(ApplicationDbContext db, UserManager<IdentityUser> users, OnlineDemoSettings.Binding binding)
{
    internal sealed record Site(Customer Customer, string AdminUserId, string NormalUserId);
    internal sealed record Scope(Tenant Tenant, Customer PrimaryCustomer, IReadOnlyList<Customer> Customers, IReadOnlyList<Relationship> Members);

    /// <summary>根据部署绑定和实际声明验证独立租户，并拒绝操作管理员或跨租户身份。</summary>
    internal async Task<Scope> VerifyAsync(CancellationToken token)
    {
        var primary = await users.FindByIdAsync(binding.DemoUserId)
            ?? throw new InvalidOperationException("演示主账号缺失。");
        var admin = await users.FindByIdAsync(binding.AdminUserId)
            ?? throw new InvalidOperationException("平台管理员缺失。");
        var adminClaims = await users.GetClaimsAsync(admin);
        if (!await users.IsInRoleAsync(admin, nameof(UserRole.SystemAdmin))
            || adminClaims.Any(c => c.Type == IoTSharpClaimTypes.Tenant && Guid.TryParse(c.Value, out var id) && id == binding.DemoTenantId))
            throw new InvalidOperationException("管理员与演示租户边界无效。");

        var tenant = await db.Tenant.SingleAsync(t => t.Id == binding.DemoTenantId && !t.Deleted, token);
        if (tenant.Email != OnlineAccountMaintenance.DemoName || primary.UserName != OnlineAccountMaintenance.DemoName
            || !await users.IsInRoleAsync(primary, nameof(UserRole.TenantAdmin)))
            throw new InvalidOperationException("固定演示主账号与租户信息不符。");

        var customers = await db.Customer.Where(c => EF.Property<Guid?>(c, "TenantId") == tenant.Id)
            .Take(201).ToListAsync(token);
        var members = await db.Relationship.Include(r => r.IdentityUser).Include(r => r.Customer)
            .Where(r => EF.Property<Guid?>(r, "TenantId") == tenant.Id).Take(201).ToListAsync(token);
        if (customers.Count > 200 || members.Count > 200)
            throw new InvalidOperationException("演示身份超出单批处理上限。");

        var customerIds = customers.Select(c => c.Id).ToArray();
        var memberIds = members.Select(r => r.IdentityUser?.Id).ToArray();
        if (memberIds.Any(id => id is null) || memberIds.Distinct().Count() != members.Count
            || await db.Relationship.AnyAsync(r =>
                (customerIds.Contains(EF.Property<Guid?>(r, "CustomerId") ?? Guid.Empty)
                    || memberIds.Contains(EF.Property<string>(r, "IdentityUserId")))
                && (EF.Property<Guid?>(r, "TenantId") == null || EF.Property<Guid?>(r, "TenantId") != tenant.Id), token))
            throw new InvalidOperationException("演示客户或用户存在跨租户关系。");

        foreach (var member in members)
        {
            token.ThrowIfCancellationRequested();
            var account = member.IdentityUser;
            var claims = await users.GetClaimsAsync(account);
            if (account.Id == admin.Id || member.Customer is null || !customerIds.Contains(member.Customer.Id)
                || await users.IsInRoleAsync(account, nameof(UserRole.SystemAdmin))
                || !OnlinePersonalTenant.HasScope(claims, IoTSharpClaimTypes.Tenant, tenant.Id)
                || !OnlinePersonalTenant.HasScope(claims, IoTSharpClaimTypes.Customer, member.Customer.Id))
                throw new InvalidOperationException("演示成员归属验证失败。");
        }

        var primaryCustomer = members.Single(r => r.IdentityUser.Id == primary.Id).Customer;
        if (primaryCustomer.Deleted)
            throw new InvalidOperationException("演示主账号的客户已删除。");
        return new(tenant, primaryCustomer, customers, members);
    }

    /// <summary>以固定 ID 校准十个客户及二十个登录身份，回收演示租户的临时账户。</summary>
    internal async Task<IReadOnlyList<Site>> RestoreAsync(Scope scope, string password, CancellationToken token)
    {
        var sites = new List<Site>(10);
        var keepUsers = new HashSet<string>(StringComparer.Ordinal) { binding.DemoUserId };
        for (var number = 1; number <= 10; number++)
        {
            token.ThrowIfCancellationRequested();
            var name = $"demo{number}";
            var id = number == 1 ? scope.PrimaryCustomer.Id : OnlineDemoSeed.StableId(scope.Tenant.Id, "customer/" + name);
            var customer = scope.Customers.SingleOrDefault(c => c.Id == id);
            if (customer is null)
            {
                if (await db.Customer.AnyAsync(c => c.Id == id, token))
                    throw new InvalidOperationException("预设客户 ID 已被占用。");
                customer = new Customer { Id = id, Tenant = scope.Tenant };
                db.Customer.Add(customer);
            }
            customer.Name = name;
            customer.Email = name + "@iotsharp.online";
            customer.Deleted = false;
            var operatorId = await RestoreUserAsync(scope, customer, $"{name}_customer{number}_user1", true, password);
            var viewerId = await RestoreUserAsync(scope, customer, $"{name}_customer{number}_user2", false, password);
            keepUsers.Add(operatorId);
            keepUsers.Add(viewerId);
            sites.Add(new(customer, operatorId, viewerId));
        }

        var extra = scope.Members.Where(r => !keepUsers.Contains(r.IdentityUser.Id)).ToArray();
        foreach (var member in extra)
        {
            token.ThrowIfCancellationRequested();
            var userId = member.IdentityUser.Id;
            await OnlineDemoCleanup.RemoveAsync(db, db.RefreshTokens.Where(t => t.UserId == userId), token);
            db.Relationship.Remove(member);
        }
        await db.SaveChangesAsync(token);
        foreach (var member in extra)
        {
            token.ThrowIfCancellationRequested();
            Ensure(await users.DeleteAsync(member.IdentityUser));
        }

        return sites;
    }

    private async Task<string> RestoreUserAsync(Scope scope, Customer customer, string name, bool isAdmin, string password)
    {
        var id = OnlineDemoSeed.StableId(scope.Tenant.Id, "user/" + name).ToString();
        var email = name + "@iotsharp.online";
        var user = await users.FindByIdAsync(id);
        var roles = isAdmin
            ? new[] { nameof(UserRole.NormalUser), nameof(UserRole.CustomerAdmin) }
            : [nameof(UserRole.NormalUser)];
        if (user is null)
        {
            user = new IdentityUser { Id = id, UserName = email, Email = email, EmailConfirmed = true };
            user.PasswordHash = users.PasswordHasher.HashPassword(user, password);
            Ensure(await users.CreateAsync(user));
            Ensure(await users.AddClaimsAsync(user, [
                new(ClaimTypes.Email, email),
                new(IoTSharpClaimTypes.Tenant, scope.Tenant.Id.ToString()),
                new(IoTSharpClaimTypes.Customer, customer.Id.ToString())
            ]));
            Ensure(await users.AddToRolesAsync(user, roles));
            db.Relationship.Add(new Relationship { Id = Guid.NewGuid(), IdentityUser = user, Tenant = scope.Tenant, Customer = customer });
            return id;
        }

        if (!scope.Members.Any(r => r.IdentityUser.Id == id && r.Customer.Id == customer.Id))
            throw new InvalidOperationException("预设演示身份已绑定其它范围。");
        var actualRoles = await users.GetRolesAsync(user);
        var removed = actualRoles.Except(roles).ToArray();
        var added = roles.Except(actualRoles).ToArray();
        if (removed.Length > 0) Ensure(await users.RemoveFromRolesAsync(user, removed));
        if (added.Length > 0) Ensure(await users.AddToRolesAsync(user, added));
        var correctPassword = users.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", password) != PasswordVerificationResult.Failed;
        if (user.UserName != email || user.Email != email || !user.EmailConfirmed || !correctPassword
            || user.LockoutEnd.HasValue || user.AccessFailedCount != 0)
        {
            user.UserName = user.Email = email;
            user.EmailConfirmed = true;
            if (!correctPassword) user.PasswordHash = users.PasswordHasher.HashPassword(user, password);
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            Ensure(await users.UpdateAsync(user));
        }
        return id;
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("演示身份恢复失败：" + string.Join(',', result.Errors.Select(error => error.Code)));
    }
}
