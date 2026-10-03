#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 验证通用设备属性入口不能修改服务器侧采集配置保留键。
/// </summary>
[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class ReservedConfigurationAttributeTests
{
    private static readonly string[] ConfigurationKeys =
    [
        Constants._EdgeCollectionConfig,
        Constants._EdgeCollectionConfigVersion,
        Constants._EdgeCollectionConfigUpdatedAt
    ];

    private readonly SqliteAppFixture _fixture;

    public ReservedConfigurationAttributeTests(SqliteAppFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 任一侧配置保留键均使整个请求失败，大小写变化也不能绕过检查。
    /// </summary>
    /// <param name="key">待验证的配置保留键。</param>
    /// <param name="uppercase">是否将请求键转为大写。</param>
    /// <param name="side">请求中包含保留键的属性侧。</param>
    [Theory(Timeout = 30000)]
    [InlineData(Constants._EdgeCollectionConfig, false, "server")]
    [InlineData(Constants._EdgeCollectionConfig, true, "server")]
    [InlineData(Constants._EdgeCollectionConfigVersion, false, "server")]
    [InlineData(Constants._EdgeCollectionConfigVersion, true, "server")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, false, "server")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, true, "server")]
    [InlineData(Constants._EdgeCollectionConfig, false, "client")]
    [InlineData(Constants._EdgeCollectionConfig, true, "client")]
    [InlineData(Constants._EdgeCollectionConfigVersion, false, "client")]
    [InlineData(Constants._EdgeCollectionConfigVersion, true, "client")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, false, "client")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, true, "client")]
    [InlineData(Constants._EdgeCollectionConfig, false, "any")]
    [InlineData(Constants._EdgeCollectionConfig, true, "any")]
    [InlineData(Constants._EdgeCollectionConfigVersion, false, "any")]
    [InlineData(Constants._EdgeCollectionConfigVersion, true, "any")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, false, "any")]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, true, "any")]
    public async Task EditAttribute_WhenAnySideContainsConfigurationKey_RejectsBeforeSaving(
        string key, bool uppercase, string side)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gateway = await SeedGatewayAsync(context);
        var controller = CreateController(scope.ServiceProvider, gateway);

        var request = new DeviceAttrEditDto
        {
            serverside = new Dictionary<string, object> { ["ordinary.server"] = "unexpected-server" },
            clientside = new Dictionary<string, object> { ["ordinary.client"] = "unexpected-client" },
            anyside = new Dictionary<string, object> { ["ordinary.any"] = "unexpected-any" }
        };
        var target = side switch
        {
            "client" => request.clientside,
            "any" => request.anyside,
            _ => request.serverside
        };
        target[uppercase ? key.ToUpperInvariant() : key] = "unexpected-config";

        var result = await controller.EditAttribute(gateway.Id, request);

        Assert.Equal((int)ApiCode.InValidData, result.Code);
        Assert.Contains("CollectionConfig", result.Msg);
        var rows = await ReadAttributesAsync(gateway.Id);
        Assert.Equal(ConfigurationKeys.Length, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Contains(row.KeyName, ConfigurationKeys);
            Assert.Equal("approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
    }

    /// <summary>
    /// 普通服务器、客户端及任意侧属性继续保存，已有服务器侧配置保持原值。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task EditAttribute_WhenKeysAreOrdinary_PreservesAttributesAndServerConfiguration()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gateway = await SeedGatewayAsync(context);
        var controller = CreateController(scope.ServiceProvider, gateway);

        var result = await controller.EditAttribute(gateway.Id, new DeviceAttrEditDto
        {
            serverside = new Dictionary<string, object> { ["ordinary.server"] = "server-value" },
            clientside = new Dictionary<string, object> { ["ordinary.client"] = "client-value" },
            anyside = new Dictionary<string, object> { ["ordinary.any"] = "any-value" }
        });

        Assert.Equal((int)ApiCode.Success, result.Code);
        var rows = await ReadAttributesAsync(gateway.Id);
        Assert.Equal("server-value", rows.Single(row => row.KeyName == "ordinary.server").Value_String);
        Assert.Equal(DataSide.ServerSide, rows.Single(row => row.KeyName == "ordinary.server").DataSide);
        Assert.Equal("client-value", rows.Single(row => row.KeyName == "ordinary.client").Value_String);
        Assert.Equal(DataSide.ClientSide, rows.Single(row => row.KeyName == "ordinary.client").DataSide);
        Assert.Equal("any-value", rows.Single(row => row.KeyName == "ordinary.any").Value_String);
        Assert.Equal(DataSide.AnySide, rows.Single(row => row.KeyName == "ordinary.any").DataSide);
        Assert.All(rows.Where(row => ConfigurationKeys.Contains(row.KeyName)), row =>
        {
            Assert.Equal("approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
    }

    /// <summary>
    /// 新增和删除入口对三种属性侧均拒绝配置保留键，大小写变化不能绕过检查。
    /// </summary>
    /// <param name="key">待验证的配置保留键。</param>
    /// <param name="uppercase">是否将请求键转为大写。</param>
    [Theory(Timeout = 30000)]
    [InlineData(Constants._EdgeCollectionConfig, false)]
    [InlineData(Constants._EdgeCollectionConfig, true)]
    [InlineData(Constants._EdgeCollectionConfigVersion, false)]
    [InlineData(Constants._EdgeCollectionConfigVersion, true)]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, false)]
    [InlineData(Constants._EdgeCollectionConfigUpdatedAt, true)]
    public async Task AddAndRemoveAttribute_WhenKeyIsReserved_RejectEverySide(string key, bool uppercase)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeded = await SeedScopedGatewayAsync(context, "same-customer", timeout.Token);
        var controller = CreateController(scope.ServiceProvider, seeded, false, timeout.Token);
        var requestKey = uppercase ? key.ToUpperInvariant() : key;

        await AssertReservedAddAndRemoveAsync(controller, seeded.Gateway.Id, requestKey, DataSide.ServerSide);
        await AssertReservedAddAndRemoveAsync(controller, seeded.Gateway.Id, requestKey, DataSide.ClientSide);
        await AssertReservedAddAndRemoveAsync(controller, seeded.Gateway.Id, requestKey, DataSide.AnySide);

        var rows = await ReadAttributesAsync(seeded.Gateway.Id, timeout.Token);
        Assert.Equal(ConfigurationKeys.Length, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Contains(row.KeyName, ConfigurationKeys);
            Assert.Equal("approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
    }

    /// <summary>
    /// 不可访问设备不能通过新增、修改或删除普通属性绕过设备管理范围。
    /// </summary>
    /// <param name="deviceScope">设备相对于当前账号的范围。</param>
    /// <param name="tenantAdmin">是否使用已有租户管理员权限。</param>
    [Theory(Timeout = 30000)]
    [InlineData("other-tenant", false)]
    [InlineData("other-customer", false)]
    [InlineData("deleted", false)]
    [InlineData("other-tenant", true)]
    [InlineData("deleted", true)]
    public async Task AttributeEndpoints_WhenDeviceIsOutsideScope_RejectWithoutChangingAttributes(
        string deviceScope, bool tenantAdmin)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeded = await SeedScopedGatewayAsync(context, deviceScope, timeout.Token);
        var controller = CreateController(scope.ServiceProvider, seeded, tenantAdmin, timeout.Token);
        context.AttributeLatest.Add(new AttributeLatest
        {
            DeviceId = seeded.Gateway.Id,
            KeyName = "ordinary.existing",
            DataSide = DataSide.ServerSide,
            Type = DataType.String,
            Value_String = "original-value"
        });
        await context.SaveChangesAsync(timeout.Token);

        var addition = await controller.AddAttribute("unused-legacy-route", new DeviceAttributeDto
        {
            DeviceId = seeded.Gateway.Id,
            KeyName = "ordinary.new",
            DataSide = DataSide.ServerSide,
            DataType = DataType.String
        });
        var edit = await controller.EditAttribute(seeded.Gateway.Id, new DeviceAttrEditDto
        {
            serverside = new Dictionary<string, object> { ["ordinary.existing"] = "unexpected-value" }
        });
        var removal = await controller.RemoveAttribute(new RemoveDeviceAttributeInput
        {
            DeviceId = seeded.Gateway.Id,
            KeyName = "ordinary.existing",
            DataSide = DataSide.ServerSide
        });

        Assert.Equal((int)ApiCode.NotFoundDevice, addition.Code);
        Assert.False(addition.Data);
        Assert.Equal((int)ApiCode.NotFoundDevice, edit.Code);
        Assert.Equal((int)ApiCode.NotFoundDevice, removal.Code);
        Assert.False(removal.Data);
        var rows = await ReadAttributesAsync(seeded.Gateway.Id, timeout.Token);
        Assert.Equal(ConfigurationKeys.Length + 1, rows.Count);
        Assert.Equal("original-value", rows.Single(row => row.KeyName == "ordinary.existing").Value_String);
        Assert.DoesNotContain(rows, row => row.KeyName == "ordinary.new");
        Assert.All(rows.Where(row => ConfigurationKeys.Contains(row.KeyName)), row =>
            Assert.Equal("approved-config", row.Value_String));
    }

    /// <summary>
    /// 当前客户和租户管理员可访问的同租户客户继续支持普通属性的完整新增、修改和删除。
    /// </summary>
    /// <param name="deviceScope">设备相对于当前账号的范围。</param>
    /// <param name="tenantAdmin">是否使用已有租户管理员权限。</param>
    [Theory(Timeout = 30000)]
    [InlineData("same-customer", false)]
    [InlineData("other-customer", true)]
    public async Task AttributeEndpoints_WhenDeviceIsInScope_PreserveOrdinaryAttributeLifecycle(
        string deviceScope, bool tenantAdmin)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeded = await SeedScopedGatewayAsync(context, deviceScope, timeout.Token);
        var controller = CreateController(scope.ServiceProvider, seeded, tenantAdmin, timeout.Token);

        var addition = await controller.AddAttribute("unused-legacy-route", new DeviceAttributeDto
        {
            DeviceId = seeded.Gateway.Id,
            KeyName = "ordinary.lifecycle",
            DataSide = DataSide.ServerSide,
            DataType = DataType.String
        });
        Assert.Equal((int)ApiCode.Success, addition.Code);
        Assert.True(addition.Data);
        var addedRows = await ReadAttributesAsync(seeded.Gateway.Id, timeout.Token);
        Assert.Contains(addedRows, row => row.KeyName == "ordinary.lifecycle" && row.DataSide == DataSide.ServerSide);

        var edit = await controller.EditAttribute(seeded.Gateway.Id, new DeviceAttrEditDto
        {
            serverside = new Dictionary<string, object> { ["ordinary.lifecycle"] = "updated-value" }
        });
        Assert.Equal((int)ApiCode.Success, edit.Code);
        var editedRows = await ReadAttributesAsync(seeded.Gateway.Id, timeout.Token);
        Assert.Equal("updated-value", editedRows.Single(row => row.KeyName == "ordinary.lifecycle").Value_String);

        var removal = await controller.RemoveAttribute(new RemoveDeviceAttributeInput
        {
            DeviceId = seeded.Gateway.Id,
            KeyName = "ordinary.lifecycle",
            DataSide = DataSide.ServerSide
        });
        Assert.Equal((int)ApiCode.Success, removal.Code);
        Assert.True(removal.Data);
        var rows = await ReadAttributesAsync(seeded.Gateway.Id, timeout.Token);
        Assert.Equal(ConfigurationKeys.Length, rows.Count);
        Assert.All(rows, row => Assert.Equal("approved-config", row.Value_String));
    }

    /// <summary>
    /// 客户端和任意侧底层写入会报告配置保留键错误，同时继续保存普通属性。
    /// </summary>
    /// <param name="side">待验证的非服务器侧来源。</param>
    /// <param name="uppercase">是否使用大写保留键。</param>
    [Theory(Timeout = 30000)]
    [InlineData(DataSide.ClientSide, false)]
    [InlineData(DataSide.ClientSide, true)]
    [InlineData(DataSide.AnySide, false)]
    [InlineData(DataSide.AnySide, true)]
    public async Task PreparingData_WhenSourceIsNotServer_ProtectsConfigurationAndKeepsOrdinaryKeys(
        DataSide side, bool uppercase)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gateway = await SeedGatewayAsync(context);
        var data = ConfigurationKeys.ToDictionary(key => uppercase ? key.ToUpperInvariant() : key,
            _ => (object)"unexpected-config");
        data["ordinary.attribute"] = "ordinary-value";

        var errors = context.PreparingData<AttributeLatest>(data, gateway.Id, side);
        Assert.Equal(ConfigurationKeys.Length, errors.Count);
        Assert.All(data.Keys.Where(key => key != "ordinary.attribute"), key => Assert.True(errors.ContainsKey(key)));
        await context.SaveChangesAsync();

        var rows = await ReadAttributesAsync(gateway.Id);
        Assert.Equal(ConfigurationKeys.Length + 1, rows.Count);
        Assert.All(rows.Where(row => ConfigurationKeys.Contains(row.KeyName)), row =>
        {
            Assert.Equal("approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
        var ordinary = rows.Single(row => row.KeyName == "ordinary.attribute");
        Assert.Equal("ordinary-value", ordinary.Value_String);
        Assert.Equal(side, ordinary.DataSide);
    }

    /// <summary>
    /// 没有明确服务器侧来源的产品属性列表不能写入配置保留键，普通定义仍会保存。
    /// </summary>
    /// <param name="uppercase">是否使用大写保留键。</param>
    [Theory(Timeout = 30000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreparingData_WhenInputIsProductAttributeList_ProtectsConfiguration(bool uppercase)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gateway = await SeedGatewayAsync(context);
        var attributes = ConfigurationKeys.Select(key => new ProductData
        {
            KeyName = uppercase ? key.ToUpperInvariant() : key,
            Value_String = "unexpected-config"
        }).ToList();
        attributes.Add(new ProductData { KeyName = "ordinary.definition" });

        var errors = context.PreparingData<AttributeLatest>(attributes, gateway.Id);
        Assert.Equal(ConfigurationKeys.Length, errors.Count);
        await context.SaveChangesAsync();

        var rows = await ReadAttributesAsync(gateway.Id);
        Assert.Equal(ConfigurationKeys.Length + 1, rows.Count);
        Assert.Contains(rows, row => row.KeyName == "ordinary.definition");
        Assert.All(rows.Where(row => ConfigurationKeys.Contains(row.KeyName)), row =>
        {
            Assert.Equal("approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
    }

    /// <summary>
    /// 专用配置入口所用的明确服务器侧写入仍可更新这三个配置属性。
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task PreparingData_WhenSourceIsServer_PreservesDedicatedConfigurationWrite()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gateway = await SeedGatewayAsync(context);

        var errors = context.PreparingData<AttributeLatest>(
            ConfigurationKeys.ToDictionary(key => key, _ => (object)"next-approved-config"), gateway.Id, DataSide.ServerSide);
        Assert.Empty(errors);
        await context.SaveChangesAsync();

        var rows = await ReadAttributesAsync(gateway.Id);
        Assert.Equal(ConfigurationKeys.Length, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal("next-approved-config", row.Value_String);
            Assert.Equal(DataSide.ServerSide, row.DataSide);
        });
    }

    /// <summary>
    /// 创建独立 Gateway 及三项已批准的服务器侧配置属性。
    /// </summary>
    /// <param name="context">当前测试数据库上下文。</param>
    /// <returns>测试账号可访问的 Gateway。</returns>
    private static async Task<Gateway> SeedGatewayAsync(ApplicationDbContext context)
    {
        var tenant = new Tenant { Name = $"reserved-config-tenant-{Guid.NewGuid():N}" };
        var customer = new Customer { Name = "reserved-config-customer", Tenant = tenant };
        var gateway = new Gateway
        {
            Name = "reserved-config-gateway",
            DeviceType = DeviceType.Gateway,
            Tenant = tenant,
            Customer = customer
        };
        context.Device.Add(gateway);
        await context.SaveChangesAsync();
        await context.SaveAsync<AttributeLatest>(ConfigurationKeys.ToDictionary(key => key, _ => (object)"approved-config"),
            gateway.Id, DataSide.ServerSide);
        return gateway;
    }

    /// <summary>
    /// 为已声明的账号范围创建目标设备和服务器配置，确保不同范围间数据独立。
    /// </summary>
    /// <param name="context">当前测试数据库上下文。</param>
    /// <param name="deviceScope">目标设备的授权范围。</param>
    /// <param name="cancellationToken">当前用例的有界取消标记。</param>
    /// <returns>账号范围和目标 Gateway。</returns>
    private static async Task<ScopedGateway> SeedScopedGatewayAsync(
        ApplicationDbContext context, string deviceScope, CancellationToken cancellationToken)
    {
        var tenant = new Tenant { Name = $"attribute-scope-tenant-{Guid.NewGuid():N}" };
        var customer = new Customer { Name = "attribute-scope-customer", Tenant = tenant };
        var targetTenant = deviceScope == "other-tenant"
            ? new Tenant { Name = $"attribute-scope-other-{Guid.NewGuid():N}" }
            : tenant;
        var targetCustomer = deviceScope is "other-tenant" or "other-customer"
            ? new Customer { Name = "attribute-scope-other-customer", Tenant = targetTenant }
            : customer;
        var gateway = new Gateway
        {
            Name = "attribute-scope-gateway",
            DeviceType = DeviceType.Gateway,
            Tenant = targetTenant,
            Customer = targetCustomer,
            Deleted = deviceScope == "deleted"
        };
        context.Customer.Add(customer);
        context.Device.Add(gateway);
        await context.SaveChangesAsync(cancellationToken);
        await context.SaveAsync<AttributeLatest>(ConfigurationKeys.ToDictionary(key => key, _ => (object)"approved-config"),
            gateway.Id, DataSide.ServerSide);
        return new ScopedGateway(tenant.Id, customer.Id, gateway);
    }

    /// <summary>
    /// 验证同一属性侧的新增和删除均在保存之前拒绝保留键。
    /// </summary>
    /// <param name="controller">具有测试账号上下文的设备控制器。</param>
    /// <param name="deviceId">目标设备标识。</param>
    /// <param name="key">包含大小写变化的配置保留键。</param>
    /// <param name="side">待验证的属性侧。</param>
    private static async Task AssertReservedAddAndRemoveAsync(
        DevicesController controller, Guid deviceId, string key, DataSide side)
    {
        var addition = await controller.AddAttribute("unused-legacy-route", new DeviceAttributeDto
        {
            DeviceId = deviceId,
            KeyName = key,
            DataSide = side,
            DataType = DataType.String
        });
        var removal = await controller.RemoveAttribute(new RemoveDeviceAttributeInput
        {
            DeviceId = deviceId,
            KeyName = key,
            DataSide = side
        });
        Assert.Equal((int)ApiCode.InValidData, addition.Code);
        Assert.False(addition.Data);
        Assert.Contains("CollectionConfig", addition.Msg);
        Assert.Equal((int)ApiCode.InValidData, removal.Code);
        Assert.False(removal.Data);
        Assert.Contains("CollectionConfig", removal.Msg);
    }

    /// <summary>
    /// 创建使用同租户客户账号的设备控制器。
    /// </summary>
    /// <param name="services">测试作用域依赖。</param>
    /// <param name="gateway">测试账号所属 Gateway。</param>
    /// <returns>已设置当前账号的控制器。</returns>
    private static DevicesController CreateController(IServiceProvider services, Gateway gateway)
        => CreateController(services, new ScopedGateway(gateway.Tenant.Id, gateway.Customer.Id, gateway), false);

    /// <summary>
    /// 将独立账号范围和已有角色写入控制器上下文，不使用目标设备的归属作为账号权限。
    /// </summary>
    /// <param name="services">测试作用域依赖。</param>
    /// <param name="seeded">账号范围和目标设备。</param>
    /// <param name="tenantAdmin">是否添加已有租户管理员角色。</param>
    /// <param name="cancellationToken">当前用例的有界取消标记。</param>
    /// <returns>已设置当前账号的控制器。</returns>
    private static DevicesController CreateController(IServiceProvider services, ScopedGateway seeded,
        bool tenantAdmin, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(IoTSharpClaimTypes.Tenant, seeded.TenantId.ToString()),
            new(IoTSharpClaimTypes.Customer, seeded.CustomerId.ToString()),
            new(ClaimTypes.Role, nameof(UserRole.NormalUser))
        };
        if (tenantAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.TenantAdmin)));
        }
        var controller = ActivatorUtilities.CreateInstance<DevicesController>(services);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestAborted = cancellationToken,
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };
        return controller;
    }

    /// <summary>
    /// 使用新作用域读取实际已保存的属性，避免跟踪缓存掩盖写入结果。
    /// </summary>
    /// <param name="gatewayId">当前测试 Gateway 标识。</param>
    /// <param name="cancellationToken">可选的有界取消标记。</param>
    /// <returns>数据库属性快照。</returns>
    private async Task<List<AttributeLatest>> ReadAttributesAsync(Guid gatewayId, CancellationToken cancellationToken = default)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.AttributeLatest.AsNoTracking().Where(row => row.DeviceId == gatewayId).ToListAsync(cancellationToken);
    }

    private sealed record ScopedGateway(Guid TenantId, Guid CustomerId, Gateway Gateway);
}
