#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using IoTSharp.Services;
using IoTSharp.Controllers;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IoTSharp.Test;

public sealed class OnlineAccountMaintenanceTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;
    private UserManager<IdentityUser> _users = null!;
    private OnlineAccountMaintenance _maintenance = null!;
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        var database = Guid.NewGuid().ToString("N");
        services.AddLogging();
        services.AddEntityFrameworkInMemoryDatabase();
        services.AddSingleton<IDataBaseModelBuilderOptions, SonnetDbModelBuilderOptions>();
        services.AddDbContext<ApplicationDbContext>((provider, options) => options.UseInMemoryDatabase(database).UseInternalServiceProvider(provider));
        services.AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _users = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Enum.GetNames<UserRole>()) Assert.True((await roles.CreateAsync(new IdentityRole(role))).Succeeded);
        var admin = new IdentityUser { UserName = "demo@iotsharp.net", Email = "demo@iotsharp.net" };
        Assert.True((await _users.CreateAsync(admin, "OriginalAdmin1!")).Succeeded);
        Assert.True((await _users.AddToRoleAsync(admin, nameof(UserRole.SystemAdmin))).Succeeded);
        Assert.True((await _users.CreateAsync(new IdentityUser { UserName = "customer@example.test", Email = "customer@example.test" }, "CustomerPass1!")).Succeeded);
        _maintenance = new OnlineAccountMaintenance(_context, _users);
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        _provider.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DemoAdministrator_CannotLockOrModifyPlatformAdministrator()
    {
        var receipt = await BootstrapAsync();
        var customer = Assert.Single(_context.Customer);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, receipt.DemoUserId),
            new Claim(ClaimTypes.Role, nameof(UserRole.CustomerAdmin)),
            new Claim(ClaimTypes.Role, nameof(UserRole.TenantAdmin)),
            new Claim(IoTSharpClaimTypes.Tenant, receipt.DemoTenantId),
            new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString())
        ], "test"));
        var controller = CreateAccountController(principal);
        var admin = (await _users.FindByIdAsync(receipt.AdminUserId!))!;
        var result = await controller.Lock(new LockDto { Id = Guid.Parse(admin.Id), Opt = LockOpt.Lock });
        Assert.Equal((int)ApiCode.DoNotAllow, result.Code);
        var modified = await controller.Modify(new UserItemDto { Id = admin.Id, PhoneNumber = "12345678901" });
        Assert.Equal((int)ApiCode.DoNotAllow, modified.Code);
        Assert.Null(admin.PhoneNumber);
        Assert.Null(admin.LockoutEnd);
    }

    [Fact]
    public async Task PublicRegistration_CannotJoinAnExistingTenantByKnowingItsEmail()
    {
        var receipt = await BootstrapAsync();
        var controller = CreateAccountController(new ClaimsPrincipal());
        var result = await controller.Create(new InstallDto
        {
            Email = "uninvited@example.test", Password = "NewCustomer1!", TenantName = "uninvited",
            CustomerName = "uninvited", TenantEMail = OnlineAccountMaintenance.DemoName,
            CustomerEMail = OnlineAccountMaintenance.DemoName
        });
        Assert.Equal((int)ApiCode.AlreadyExists, result.Code);
        Assert.Null(await _users.FindByNameAsync("uninvited@example.test"));
        Assert.Single(_context.Tenant);
        Assert.NotNull(await _users.FindByIdAsync(receipt.DemoUserId));
    }

    [Fact]
    public async Task CustomerAdministrator_CreatesOnlyNormalUsersWithinOwnCustomer()
    {
        var receipt = await BootstrapAsync();
        var customer = Assert.Single(_context.Customer);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, nameof(UserRole.CustomerAdmin)),
            new Claim(IoTSharpClaimTypes.Tenant, receipt.DemoTenantId),
            new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString())
        ], "test"));
        var controller = CreateAccountController(principal);
        var result = await controller.PostAccount(new CreateUserInput
        {
            Email = "invited@example.test", Password = "InvitedUser1!", CustomerId = customer.Id
        });
        Assert.Equal((int)ApiCode.Success, result.Code);
        var invited = (await _users.FindByNameAsync("invited@example.test"))!;
        Assert.Equal([nameof(UserRole.NormalUser)], await _users.GetRolesAsync(invited));
        var otherTenant = new Tenant { Name = "other", Email = "other@example.test" };
        var otherCustomer = new Customer { Name = "other", Email = "other@example.test", Tenant = otherTenant };
        _context.Customer.Add(otherCustomer);
        await _context.SaveChangesAsync();
        var denied = await controller.PostAccount(new CreateUserInput
        {
            Email = "cross-tenant@example.test", Password = "InvitedUser1!", CustomerId = otherCustomer.Id
        });
        Assert.Equal((int)ApiCode.DoNotAllow, denied.Code);
        Assert.Null(await _users.FindByNameAsync("cross-tenant@example.test"));
    }

    [Fact]
    public async Task PublicRegistration_CreatesIndependentTenantWithoutSystemAdministrator()
    {
        var controller = CreateAccountController(new ClaimsPrincipal());
        var result = await controller.Create(new InstallDto
        {
            Email = "new@example.test", Password = "NewCustomer1!", TenantName = "new", CustomerName = "new",
            TenantEMail = "tenant@example.test", CustomerEMail = "business@example.test"
        });
        Assert.Equal((int)ApiCode.Success, result.Code);
        var user = (await _users.FindByNameAsync("new@example.test"))!;
        Assert.True(await _users.IsInRoleAsync(user, nameof(UserRole.TenantAdmin)));
        Assert.False(await _users.IsInRoleAsync(user, nameof(UserRole.SystemAdmin)));
        Assert.Single(_context.Tenant);
    }

    [Fact]
    public async Task Bootstrap_PreservesAdminIdentityAndCreatesIsolatedNonSystemDemo()
    {
        var previousAdminId = (await _users.FindByNameAsync("demo@iotsharp.net"))!.Id;
        var receipt = await BootstrapAsync();
        var admin = (await _users.FindByNameAsync(OnlineAccountMaintenance.AdminName))!;
        var demo = (await _users.FindByIdAsync(receipt.DemoUserId))!;
        Assert.Equal(previousAdminId, receipt.AdminUserId);
        Assert.True(await _users.CheckPasswordAsync(admin, "operator-chosen1"));
        Assert.True(_users.Options.Password.RequireUppercase);
        Assert.Null(await _users.FindByNameAsync("demo@iotsharp.net"));
        Assert.False(await _users.IsInRoleAsync(demo, nameof(UserRole.SystemAdmin)));
        Assert.True(await _users.IsInRoleAsync(demo, nameof(UserRole.TenantAdmin)));
        Assert.Equal(OnlineAccountMaintenance.DemoName, Assert.Single(_context.Tenant).Email);
        Assert.Single(_context.Relationship);
    }

    [Fact]
    public async Task ResetDemo_RestoresRenamedAccountWithoutChangingOtherCredentialsOrData()
    {
        var receipt = await BootstrapAsync();
        var before = await OtherCredentialsAsync(receipt.DemoUserId);
        var demo = (await _users.FindByIdAsync(receipt.DemoUserId))!;
        Assert.True((await _users.ChangePasswordAsync(demo, "DemoOriginal1!", "ChangedDemo2!")).Succeeded);
        demo.UserName = "changed@example.test";
        demo.Email = "changed@example.test";
        Assert.True((await _users.UpdateAsync(demo)).Succeeded);
        var tenantCount = await _context.Tenant.CountAsync();
        var customerCount = await _context.Customer.CountAsync();

        await _maintenance.ExecuteAsync(new OnlineAccountMaintenance.Request
        {
            Mode = "reset-demo", DemoUserId = receipt.DemoUserId,
            DemoTenantId = receipt.DemoTenantId, DemoPassword = "DemoOriginal1!"
        });

        Assert.Equal(before, await OtherCredentialsAsync(receipt.DemoUserId));
        Assert.True(await _users.CheckPasswordAsync(demo, "DemoOriginal1!"));
        Assert.Equal(OnlineAccountMaintenance.DemoName, demo.UserName);
        Assert.Equal(OnlineAccountMaintenance.DemoName, demo.Email);
        Assert.Equal(tenantCount, await _context.Tenant.CountAsync());
        Assert.Equal(customerCount, await _context.Customer.CountAsync());
    }

    [Fact]
    public async Task ResetDemo_RejectsAdministratorIdBeforeChangingItsPassword()
    {
        var receipt = await BootstrapAsync();
        var admin = (await _users.FindByIdAsync(receipt.AdminUserId!))!;
        var previousHash = admin.PasswordHash;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenance.ExecuteAsync(new OnlineAccountMaintenance.Request
        {
            Mode = "reset-demo", DemoUserId = admin.Id,
            DemoTenantId = receipt.DemoTenantId, DemoPassword = "DemoOriginal1!"
        }));
        Assert.Equal(previousHash, admin.PasswordHash);
    }

    [Fact]
    public async Task ResetDemo_RejectsChangedTenantBinding()
    {
        var receipt = await BootstrapAsync();
        var demo = (await _users.FindByIdAsync(receipt.DemoUserId))!;
        var previousHash = demo.PasswordHash;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenance.ExecuteAsync(new OnlineAccountMaintenance.Request
        {
            Mode = "reset-demo", DemoUserId = demo.Id,
            DemoTenantId = Guid.NewGuid().ToString(), DemoPassword = "DemoOriginal1!"
        }));
        Assert.Equal(previousHash, demo.PasswordHash);
    }

    private Task<OnlineAccountMaintenance.Receipt> BootstrapAsync() => _maintenance.ExecuteAsync(new OnlineAccountMaintenance.Request
    {
        Mode = "bootstrap", DemoPassword = "DemoOriginal1!", AdminPassword = "operator-chosen1"
    });

    private AccountController CreateAccountController(ClaimsPrincipal principal) => new(
        _users, null!, NullLogger<AccountController>.Instance, _context, Options.Create(new AppSettings()), null!)
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } }
    };

    private async Task<string[]> OtherCredentialsAsync(string demoId) => await _context.Users.Where(u => u.Id != demoId)
        .OrderBy(u => u.Id).Select(u => u.Id + ":" + u.UserName + ":" + u.PasswordHash + ":" + u.SecurityStamp).ToArrayAsync();
}
