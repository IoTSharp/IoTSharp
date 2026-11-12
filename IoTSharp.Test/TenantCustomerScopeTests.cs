#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 回归验证租户和客户管理入口的租户边界及请求对象绑定保护。
/// </summary>
[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class TenantCustomerScopeTests
{
    private readonly SqliteAppFixture _fixture;

    /// <summary>
    /// 复用 SQLite 应用测试夹具，使用独立作用域读取持久化结果。
    /// </summary>
    /// <param name="fixture">应用测试夹具。</param>
    public TenantCustomerScopeTests(SqliteAppFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 租户管理员不能读取或修改其他租户，且同租户资料更新不会覆盖身份和导航属性。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task TenantEndpoints_EnforceTenantScope_AndProtectIdentityFields()
    {
        var seeded = await SeedAsync();
        using var scope = _fixture.Services.CreateScope();
        var principal = Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.TenantAdmin);
        var controller = CreateController<TenantsController>(scope.ServiceProvider, principal);

        var crossRead = await controller.GetTenant(seeded.TenantB.Id);
        AssertDenied(crossRead);

        var crossWrite = await controller.PutTenant(seeded.TenantB.Id, new Tenant
        {
            Id = seeded.TenantB.Id,
            Name = "attacker-tenant",
            Deleted = true,
            AISettings = new AISettings { Name = "attacker-settings" }
        });
        AssertDenied(crossWrite);

        var ownRead = await controller.GetTenant(seeded.TenantA.Id);
        Assert.Equal((int)ApiCode.Success, ownRead.Code);
        Assert.NotNull(ownRead.Data);

        var ownWrite = await controller.PutTenant(seeded.TenantA.Id, new Tenant
        {
            Id = seeded.TenantA.Id,
            Name = "updated-tenant",
            Email = "updated@example.test",
            Deleted = true,
            AISettings = new AISettings { Name = "attacker-settings" }
        });
        Assert.Equal((int)ApiCode.Success, ownWrite.Code);
        Assert.NotNull(ownWrite.Data);

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenantA = await context.Tenant.Include(item => item.AISettings).SingleAsync(item => item.Id == seeded.TenantA.Id);
        var tenantB = await context.Tenant.Include(item => item.AISettings).SingleAsync(item => item.Id == seeded.TenantB.Id);
        Assert.Equal("updated-tenant", tenantA.Name);
        Assert.Equal("updated@example.test", tenantA.Email);
        Assert.False(tenantA.Deleted);
        Assert.Equal(seeded.TenantASettingsId, tenantA.AISettings?.Id);
        Assert.Equal("tenant-a-settings", tenantA.AISettings?.Name);
        Assert.Equal("tenant-b", tenantB.Name);
        Assert.False(tenantB.Deleted);
        Assert.Equal(seeded.TenantBSettingsId, tenantB.AISettings?.Id);
    }

    /// <summary>
    /// 系统管理员保留既有跨租户管理能力，软删除租户仍然不可通过这两个入口访问。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task TenantEndpoints_SystemAdministratorRetainsCrossTenantAccess_ButDeletedIsHidden()
    {
        var seeded = await SeedAsync();
        using var scope = _fixture.Services.CreateScope();
        var controller = CreateController<TenantsController>(scope.ServiceProvider, Principal(null, null, UserRole.SystemAdmin));

        var read = await controller.GetTenant(seeded.TenantB.Id);
        Assert.Equal((int)ApiCode.Success, read.Code);
        Assert.NotNull(read.Data);

        var write = await controller.PutTenant(seeded.TenantB.Id, new Tenant { Id = seeded.TenantB.Id, Name = "system-updated" });
        Assert.Equal((int)ApiCode.Success, write.Code);

        using (var updateScope = _fixture.Services.CreateScope())
        {
            var context = updateScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var target = await context.Tenant.SingleAsync(item => item.Id == seeded.TenantB.Id);
            target.Deleted = true;
            await context.SaveChangesAsync();
        }

        using var deletedScope = _fixture.Services.CreateScope();
        var deletedController = CreateController<TenantsController>(deletedScope.ServiceProvider, Principal(null, null, UserRole.SystemAdmin));
        AssertDenied(await deletedController.GetTenant(seeded.TenantB.Id));
        AssertDenied(await deletedController.PutTenant(seeded.TenantB.Id, new Tenant { Id = seeded.TenantB.Id, Name = "must-not-update" }));
    }

    /// <summary>
    /// 租户声明缺失、格式错误或为空时必须失败关闭，而不能把异常转换成跨租户访问。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task TenantEndpoints_InvalidTenantClaimsFailClosed()
    {
        var seeded = await SeedAsync();
        var values = new string?[] { null, string.Empty, "not-a-guid", Guid.Empty.ToString() };
        foreach (var value in values)
        {
            using var scope = _fixture.Services.CreateScope();
            var claims = new List<Claim> { new(ClaimTypes.Role, nameof(UserRole.TenantAdmin)) };
            if (value is not null)
            {
                claims.Add(new Claim(IoTSharpClaimTypes.Tenant, value));
            }

            var controller = CreateController<TenantsController>(scope.ServiceProvider, new ClaimsPrincipal(new ClaimsIdentity(claims, "test")));
            var readException = await Record.ExceptionAsync(async () => AssertDenied(await controller.GetTenant(seeded.TenantA.Id)));
            Assert.Null(readException);
            var writeException = await Record.ExceptionAsync(async () => AssertDenied(await controller.PutTenant(
                seeded.TenantA.Id, new Tenant { Id = seeded.TenantA.Id, Name = "must-not-update" })));
            Assert.Null(writeException);
        }

        using (var duplicateScope = _fixture.Services.CreateScope())
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, nameof(UserRole.NormalUser)),
                new Claim(ClaimTypes.Role, nameof(UserRole.TenantAdmin)),
                new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()),
                new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantB.Id.ToString())
            }, "test"));
            var controller = CreateController<TenantsController>(duplicateScope.ServiceProvider, principal);
            AssertDenied(await controller.GetTenant(seeded.TenantA.Id));
            AssertDenied(await controller.PutTenant(seeded.TenantA.Id,
                new Tenant { Id = seeded.TenantA.Id, Name = "must-not-update" }));
        }

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await context.Tenant.SingleAsync(item => item.Id == seeded.TenantA.Id);
        Assert.Equal(seeded.TenantA.Name, persisted.Name);
        Assert.False(persisted.Deleted);
    }

    /// <summary>
    /// 客户管理员只能访问自己的客户；租户管理员可以访问本租户客户，跨租户和软删除对象均被拒绝。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task CustomerEndpoints_EnforceTenantAndCustomerScope_AndProtectIdentityFields()
    {
        var seeded = await SeedAsync();

        using (var customerScope = _fixture.Services.CreateScope())
        {
            var customerController = CreateController<CustomersController>(customerScope.ServiceProvider,
                Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.CustomerAdmin));
            Assert.Equal((int)ApiCode.Success, (await customerController.GetCustomer(seeded.CustomerA.Id)).Code);
            AssertDenied(await customerController.GetCustomer(seeded.CustomerA2.Id));
            AssertDenied(await customerController.GetCustomer(seeded.CustomerB.Id));

            var injectedWrite = await customerController.PutCustomer(seeded.CustomerA.Id, new CustomerDto
            {
                Id = seeded.CustomerA.Id,
                Name = "attacker-customer",
                TenantId = Guid.Empty,
                Tenant = new Tenant { Id = seeded.TenantB.Id },
                Deleted = true,
                AISettings = new AISettings { Name = "attacker-settings" }
            });
            Assert.Equal((int)ApiCode.Success, injectedWrite.Code);
            Assert.NotNull(injectedWrite.Data);
            var ownWrite = await customerController.PutCustomer(seeded.CustomerA.Id, new CustomerDto
            {
                Id = seeded.CustomerA.Id,
                Name = "updated-customer",
                Email = "updated-customer@example.test",
                TenantId = Guid.Empty
            });
            Assert.Equal((int)ApiCode.Success, ownWrite.Code);
            Assert.NotNull(ownWrite.Data);

            AssertDenied(await customerController.PutCustomer(seeded.CustomerA2.Id, new CustomerDto
            {
                Id = seeded.CustomerA2.Id,
                Name = "same-tenant-cross-customer",
                TenantId = seeded.TenantA.Id
            }));
        }

        using (var tenantScope = _fixture.Services.CreateScope())
        {
            using (var deletionScope = _fixture.Services.CreateScope())
            {
                var deletionContext = deletionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var deletedCustomer = await deletionContext.Customer.SingleAsync(item => item.Id == seeded.CustomerB.Id);
                deletedCustomer.Deleted = true;
                await deletionContext.SaveChangesAsync();
            }

            var tenantController = CreateController<CustomersController>(tenantScope.ServiceProvider,
                Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.TenantAdmin));
            var sameTenantRead = await tenantController.GetCustomer(seeded.CustomerA2.Id);
            Assert.Equal((int)ApiCode.Success, sameTenantRead.Code);
            Assert.NotNull(sameTenantRead.Data);

            var sameTenantWrite = await tenantController.PutCustomer(seeded.CustomerA2.Id, new CustomerDto
            {
                Id = seeded.CustomerA2.Id,
                Name = "tenant-admin-updated",
                TenantId = Guid.Empty
            });
            Assert.Equal((int)ApiCode.Success, sameTenantWrite.Code);
            Assert.NotNull(sameTenantWrite.Data);
            AssertDenied(await tenantController.GetCustomer(seeded.CustomerB.Id));
        }

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerA = await context.Customer.Include(item => item.Tenant).Include(item => item.AISettings)
            .SingleAsync(item => item.Id == seeded.CustomerA.Id);
        var customerA2 = await context.Customer.Include(item => item.Tenant)
            .SingleAsync(item => item.Id == seeded.CustomerA2.Id);
        var customerB = await context.Customer.SingleAsync(item => item.Id == seeded.CustomerB.Id);
        Assert.Equal("updated-customer", customerA.Name);
        Assert.Equal(seeded.TenantA.Id, customerA.Tenant?.Id);
        Assert.False(customerA.Deleted);
        Assert.Equal(seeded.CustomerASettingsId, customerA.AISettings?.Id);
        Assert.Equal("customer-a-settings", customerA.AISettings?.Name);
        Assert.Equal("tenant-admin-updated", customerA2.Name);
        Assert.Equal(seeded.TenantA.Id, customerA2.Tenant?.Id);
        Assert.False(customerA2.Deleted);
        Assert.Equal("customer-b", customerB.Name);
        Assert.True(customerB.Deleted);
    }

    /// <summary>
    /// 系统管理员保留跨租户客户读取和更新能力，但请求体的租户导航和归属字段不能迁移客户。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task CustomerEndpoints_SystemAdministratorRetainsCrossTenantAccess_WithoutReparenting()
    {
        var seeded = await SeedAsync();
        using var scope = _fixture.Services.CreateScope();
        var controller = CreateController<CustomersController>(scope.ServiceProvider, Principal(null, null, UserRole.SystemAdmin));

        var read = await controller.GetCustomer(seeded.CustomerB.Id);
        Assert.Equal((int)ApiCode.Success, read.Code);
        Assert.NotNull(read.Data);

        var injectedWrite = await controller.PutCustomer(seeded.CustomerB.Id, new CustomerDto
        {
            Id = seeded.CustomerB.Id,
            Name = "must-not-reparent",
            TenantId = Guid.Empty,
            Tenant = new Tenant { Id = seeded.TenantA.Id },
            Deleted = true
        });
        Assert.Equal((int)ApiCode.Success, injectedWrite.Code);
        Assert.NotNull(injectedWrite.Data);
        var write = await controller.PutCustomer(seeded.CustomerB.Id, new CustomerDto
        {
            Id = seeded.CustomerB.Id,
            Name = "system-customer-update",
            TenantId = seeded.TenantB.Id
        });
        Assert.Equal((int)ApiCode.Success, write.Code);

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customer = await context.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerB.Id);
        Assert.Equal("system-customer-update", customer.Name);
        Assert.Equal(seeded.TenantB.Id, customer.Tenant?.Id);
        Assert.False(customer.Deleted);
    }

    /// <summary>
    /// 客户管理员的租户或客户声明缺失、格式错误、为空时必须失败关闭且不抛出未处理异常。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task CustomerEndpoints_InvalidClaimsFailClosed()
    {
        var seeded = await SeedAsync();
        var claimsCases = new[]
        {
            Array.Empty<Claim>(),
            new[] { new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA.Id.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, Guid.Empty.ToString()), new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA.Id.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, "not-a-guid"), new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA.Id.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()), new Claim(IoTSharpClaimTypes.Customer, Guid.Empty.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()), new Claim(IoTSharpClaimTypes.Customer, "not-a-guid") },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()), new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantB.Id.ToString()), new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA.Id.ToString()) },
            new[] { new Claim(IoTSharpClaimTypes.Tenant, seeded.TenantA.Id.ToString()), new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA.Id.ToString()), new Claim(IoTSharpClaimTypes.Customer, seeded.CustomerA2.Id.ToString()) }
        };

        foreach (var claimSet in claimsCases)
        {
            using var scope = _fixture.Services.CreateScope();
            var claims = claimSet.ToList();
            claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.CustomerAdmin)));
            var controller = CreateController<CustomersController>(scope.ServiceProvider,
                new ClaimsPrincipal(new ClaimsIdentity(claims, "test")));
            var readException = await Record.ExceptionAsync(async () => AssertDenied(await controller.GetCustomer(seeded.CustomerA.Id)));
            Assert.Null(readException);
            var writeException = await Record.ExceptionAsync(async () => AssertDenied(await controller.PutCustomer(
                seeded.CustomerA.Id, new CustomerDto { Id = seeded.CustomerA.Id, Name = "must-not-update" })));
            Assert.Null(writeException);
        }

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await context.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerA.Id);
        Assert.Equal("customer-a", persisted.Name);
        Assert.Equal(seeded.TenantA.Id, persisted.Tenant.Id);
        Assert.False(persisted.Deleted);
    }

    /// <summary>
    /// 两个修改入口都必须拒绝路由 ID 与请求体 ID 不一致，并返回空数据。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task PutEndpoints_RejectRouteAndBodyIdMismatch()
    {
        var seeded = await SeedAsync();
        using var scope = _fixture.Services.CreateScope();

        var tenantController = CreateController<TenantsController>(scope.ServiceProvider,
            Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.TenantAdmin));
        var tenantResult = await tenantController.PutTenant(seeded.TenantA.Id, new Tenant { Id = seeded.TenantB.Id, Name = "must-not-update" });
        AssertDenied(tenantResult);

        var customerController = CreateController<CustomersController>(scope.ServiceProvider,
            Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.CustomerAdmin));
        var customerResult = await customerController.PutCustomer(seeded.CustomerA.Id, new CustomerDto { Id = seeded.CustomerA2.Id, Name = "must-not-update" });
        AssertDenied(customerResult);
    }

    /// <summary>
    /// 即便知道其他租户的 ID，也不能通过客户资料更新迁移归属或修改目标租户。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task PutCustomer_RejectsTenantIdReparenting_WithoutChangingPersistedData()
    {
        var seeded = await SeedAsync();
        using var scope = _fixture.Services.CreateScope();
        var controller = CreateController<CustomersController>(scope.ServiceProvider,
            Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.CustomerAdmin));
        AssertDenied(await controller.PutCustomer(seeded.CustomerA.Id, new CustomerDto
        {
            Id = seeded.CustomerA.Id,
            TenantId = seeded.TenantB.Id,
            Name = "must-not-reparent"
        }));
        AssertDenied(await controller.PutCustomer(seeded.CustomerB.Id, new CustomerDto
        {
            Id = seeded.CustomerB.Id,
            TenantId = seeded.TenantB.Id,
            Name = "must-not-cross-tenant"
        }));
        AssertDenied(await controller.PutCustomer(seeded.CustomerB.Id, new CustomerDto
        {
            Id = seeded.CustomerB.Id,
            TenantId = seeded.TenantA.Id,
            Name = "must-not-cross-tenant"
        }));

        using var verificationScope = _fixture.Services.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerA = await context.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerA.Id);
        var customerB = await context.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerB.Id);
        Assert.Equal("customer-a", customerA.Name);
        Assert.Equal(seeded.TenantA.Id, customerA.Tenant?.Id);
        Assert.Equal("customer-b", customerB.Name);
        Assert.Equal(seeded.TenantB.Id, customerB.Tenant?.Id);
    }

    /// <summary>
    /// 客户或其租户被软删除后，合法的当前账号也不能读取或修改资料。
    /// </summary>
    /// <param name="deleteTenant">是否删除客户所属租户而不是客户本身。</param>
    [Theory(Timeout = 30000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CustomerEndpoints_HideDeletedCustomerOrTenant(bool deleteTenant)
    {
        var seeded = await SeedAsync();
        using (var deletionScope = _fixture.Services.CreateScope())
        {
            var context = deletionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (deleteTenant)
            {
                var tenant = await context.Tenant.SingleAsync(item => item.Id == seeded.TenantA.Id);
                tenant.Deleted = true;
            }
            else
            {
                var customer = await context.Customer.SingleAsync(item => item.Id == seeded.CustomerA.Id);
                customer.Deleted = true;
            }
            await context.SaveChangesAsync();
        }

        using var scope = _fixture.Services.CreateScope();
        var controller = CreateController<CustomersController>(scope.ServiceProvider,
            Principal(seeded.TenantA.Id, seeded.CustomerA.Id, UserRole.CustomerAdmin));
        AssertDenied(await controller.GetCustomer(seeded.CustomerA.Id));
        AssertDenied(await controller.PutCustomer(seeded.CustomerA.Id, new CustomerDto
        {
            Id = seeded.CustomerA.Id,
            Name = "must-not-update"
        }));

        using var verificationScope = _fixture.Services.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await verificationContext.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerA.Id);
        Assert.Equal("customer-a", persisted.Name);
        Assert.Equal(deleteTenant, persisted.Tenant.Deleted);
        Assert.Equal(!deleteTenant, persisted.Deleted);
    }

    /// <summary>
    /// 用正式注册和登录获得的 JWT 走完整 HTTP 管道，回归公告中的四个越权入口。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task HttpEndpoints_PubliclyRegisteredAdministratorCannotReadOrOverwriteAnotherTenant()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var token = cancellation.Token;
        var seeded = await SeedAsync();
        var email = $"scope-{Guid.NewGuid():N}@example.test";
        using var client = _fixture.CreateClient();
        using (var registration = await client.PostAsJsonAsync("/api/Account/Create", new InstallDto
        {
            Email = email,
            Password = "ScopedRegression1!",
            TenantName = "scope-http-tenant",
            CustomerName = "scope-http-customer",
            TenantEMail = email,
            CustomerEMail = email,
            PhoneNumber = "18900000000"
        }, token))
        {
            var result = await ReadAsync<LoginResult>(registration, token);
            Assert.Equal((int)ApiCode.Success, result.Code);
        }

        var captchaId = Guid.NewGuid().ToString("N");
        Guid ownTenantId;
        Guid ownCustomerId;
        using (var scope = _fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ownTenantId = await context.Tenant.Where(item => item.Email == email).Select(item => item.Id).SingleAsync(token);
            ownCustomerId = await context.Customer.Where(item => item.Email == email).Select(item => item.Id).SingleAsync(token);
            var caching = scope.ServiceProvider.GetRequiredService<IEasyCachingProvider>();
            var items = caching.Get<List<ModelCaptchaVertifyItem>>("Captcha").Value ?? [];
            items.Add(new ModelCaptchaVertifyItem { Clientid = captchaId, Move = 80 });
            caching.Set("Captcha", items, TimeSpan.FromMinutes(2));
        }

        using (var login = await client.PostAsJsonAsync("/api/Account/Login", new LoginDto
        {
            UserName = email,
            Password = "ScopedRegression1!",
            CaptchaClientId = captchaId,
            CaptchaMove = 80
        }, token))
        {
            var result = await ReadAsync<LoginResult>(login, token);
            Assert.Equal((int)ApiCode.Success, result.Code);
            Assert.True(result.Data?.Succeeded);
            Assert.NotNull(result.Data?.Token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Data!.Token!.access_token);
        }

        using (var tenantRead = await client.GetAsync($"/api/Tenants/{seeded.TenantB.Id}", token))
        {
            AssertDenied(await ReadAsync<Tenant>(tenantRead, token));
        }
        using (var tenantWrite = await client.PutAsJsonAsync($"/api/Tenants/{seeded.TenantB.Id}",
            new Tenant { Id = seeded.TenantB.Id, Name = "http-cross-tenant" }, token))
        {
            AssertDenied(await ReadAsync<Tenant>(tenantWrite, token));
        }
        using (var customerRead = await client.GetAsync($"/api/Customers/{seeded.CustomerB.Id}", token))
        {
            AssertDenied(await ReadAsync<Customer>(customerRead, token));
        }
        using (var customerWrite = await client.PutAsJsonAsync($"/api/Customers/{seeded.CustomerB.Id}",
            new CustomerDto { Id = seeded.CustomerB.Id, TenantId = seeded.TenantB.Id, Name = "http-cross-customer" }, token))
        {
            AssertDenied(await ReadAsync<Customer>(customerWrite, token));
        }

        using (var ownTenantRead = await client.GetAsync($"/api/Tenants/{ownTenantId}", token))
        {
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<Tenant>(ownTenantRead, token)).Code);
        }
        using (var ownCustomerRead = await client.GetAsync($"/api/Customers/{ownCustomerId}", token))
        {
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<Customer>(ownCustomerRead, token)).Code);
        }
        using (var ownTenantWrite = await client.PutAsJsonAsync($"/api/Tenants/{ownTenantId}",
            new Tenant { Id = ownTenantId, Name = "http-own-tenant", Email = email }, token))
        {
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<Tenant>(ownTenantWrite, token)).Code);
        }
        using (var ownCustomerWrite = await client.PutAsJsonAsync($"/api/Customers/{ownCustomerId}",
            new CustomerDto { Id = ownCustomerId, TenantId = ownTenantId, Name = "http-own-customer", Email = email }, token))
        {
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<Customer>(ownCustomerWrite, token)).Code);
        }

        using var verificationScope = _fixture.Services.CreateScope();
        var verification = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal("tenant-b", (await verification.Tenant.SingleAsync(item => item.Id == seeded.TenantB.Id, token)).Name);
        var targetCustomer = await verification.Customer.Include(item => item.Tenant).SingleAsync(item => item.Id == seeded.CustomerB.Id, token);
        Assert.Equal("customer-b", targetCustomer.Name);
        Assert.Equal(seeded.TenantB.Id, targetCustomer.Tenant.Id);
        Assert.Equal("http-own-tenant", (await verification.Tenant.SingleAsync(item => item.Id == ownTenantId, token)).Name);
        Assert.Equal("http-own-customer", (await verification.Customer.SingleAsync(item => item.Id == ownCustomerId, token)).Name);
    }

    /// <summary>
    /// 确认响应经过 HTTP 管道并读取现有 ApiResult 合同，不能把认证错误页面当成业务拒绝。
    /// </summary>
    private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        return Assert.IsType<ApiResult<T>>(await response.Content.ReadFromJsonAsync<ApiResult<T>>(cancellationToken));
    }

    /// <summary>
    /// 复用真实控制器依赖并附加测试账号；角色授权中间件由独立 HTTP 测试覆盖。
    /// </summary>
    private static TController CreateController<TController>(IServiceProvider services, ClaimsPrincipal principal)
        where TController : ControllerBase
    {
        var controller = ActivatorUtilities.CreateInstance<TController>(services);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return controller;
    }

    /// <summary>
    /// 使用正式 claim 类型构造明确的租户、客户和角色范围。
    /// </summary>
    private static ClaimsPrincipal Principal(Guid? tenant, Guid? customer, params UserRole[] roles)
    {
        // 与注册和初始化流程的角色组合一致，避免把直调成功误认为单个角色可通过 HTTP 授权。
        var grantedRoles = roles.SelectMany(role => role switch
        {
            UserRole.SystemAdmin => new[] { UserRole.NormalUser, UserRole.CustomerAdmin, UserRole.TenantAdmin, UserRole.SystemAdmin },
            UserRole.TenantAdmin => new[] { UserRole.NormalUser, UserRole.CustomerAdmin, UserRole.TenantAdmin },
            UserRole.CustomerAdmin => new[] { UserRole.NormalUser, UserRole.CustomerAdmin },
            _ => new[] { role }
        });
        var claims = grantedRoles.Distinct().Select(role => new Claim(ClaimTypes.Role, role.ToString())).ToList();
        if (tenant.HasValue)
        {
            claims.Add(new Claim(IoTSharpClaimTypes.Tenant, tenant.Value.ToString()));
        }

        if (customer.HasValue)
        {
            claims.Add(new Claim(IoTSharpClaimTypes.Customer, customer.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    /// <summary>
    /// 拒绝结果必须既不是成功，也不能返回请求体或数据库实体。
    /// </summary>
    private static void AssertDenied<T>(ApiResult<T> result)
    {
        Assert.NotEqual((int)ApiCode.Success, result.Code);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// 每个测试创建独立的两个租户和三个客户，持久化后释放种子作用域。
    /// </summary>
    private async Task<SeededGraph> SeedAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenantASettings = new AISettings { Name = "tenant-a-settings" };
        var tenantBSettings = new AISettings { Name = "tenant-b-settings" };
        var customerASettings = new AISettings { Name = "customer-a-settings" };
        var tenantA = new Tenant { Name = $"tenant-a-{Guid.NewGuid():N}", Email = "tenant-a@example.test", AISettings = tenantASettings };
        var tenantB = new Tenant { Name = "tenant-b", Email = "tenant-b@example.test", AISettings = tenantBSettings };
        var customerA = new Customer { Name = "customer-a", Email = "customer-a@example.test", Tenant = tenantA, AISettings = customerASettings };
        var customerA2 = new Customer { Name = "customer-a2", Email = "customer-a2@example.test", Tenant = tenantA };
        var customerB = new Customer { Name = "customer-b", Email = "customer-b@example.test", Tenant = tenantB };
        context.Tenant.AddRange(tenantA, tenantB);
        context.Customer.AddRange(customerA, customerA2, customerB);
        await context.SaveChangesAsync();
        return new SeededGraph(tenantA, tenantB, customerA, customerA2, customerB,
            tenantASettings.Id, tenantBSettings.Id, customerASettings.Id);
    }

    private sealed record SeededGraph(
        Tenant TenantA,
        Tenant TenantB,
        Customer CustomerA,
        Customer CustomerA2,
        Customer CustomerB,
        Guid TenantASettingsId,
        Guid TenantBSettingsId,
        Guid CustomerASettingsId);
}
