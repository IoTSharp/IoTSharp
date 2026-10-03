#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IoTSharp.Test;

/// <summary>
/// 验证设备证书入口沿用设备管理的租户、客户和未删除范围。
/// </summary>
[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class DeviceCertificateScopeTests
{
    private readonly SqliteAppFixture _fixture;

    public DeviceCertificateScopeTests(SqliteAppFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 不可访问设备既不能生成新证书，也不能下载已有私钥。
    /// </summary>
    /// <param name="deviceScope">设备相对于当前账号的范围。</param>
    /// <param name="tenantAdmin">是否保留租户管理员的跨客户查询能力。</param>
    [Theory(Timeout = 30000)]
    [InlineData("other-tenant", false)]
    [InlineData("other-customer", false)]
    [InlineData("deleted", false)]
    [InlineData("other-tenant", true)]
    [InlineData("deleted", true)]
    public async Task CertificateEndpoints_WhenDeviceIsOutsideScope_RejectWithoutChangingIdentity(
        string deviceScope, bool tenantAdmin)
    {
        using var certificates = new TestCertificateFiles();
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeded = await SeedDeviceAsync(context, deviceScope);
        var controller = CreateController(scope.ServiceProvider, certificates.Settings, seeded, tenantAdmin);

        var creation = await controller.CreateX509Identity(seeded.DeviceId);
        Assert.Equal((int)ApiCode.ExceptionDeviceIdentity, creation.Code);
        Assert.Null(creation.Data);

        var download = Assert.IsType<OkObjectResult>(await controller.DownloadCertificates(seeded.DeviceId));
        var downloadResult = Assert.IsType<ApiResult>(download.Value);
        Assert.Equal((int)ApiCode.NotFoundDevice, downloadResult.Code);

        using var verificationScope = _fixture.Services.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var identity = await verificationContext.DeviceIdentities.AsNoTracking()
            .SingleAsync(item => item.Id == seeded.IdentityId);
        Assert.Equal("original-thumbprint", identity.IdentityId);
        Assert.Equal(seeded.IdentityValue, identity.IdentityValue);
    }

    /// <summary>
    /// 当前客户和租户管理员可访问的同租户客户继续支持证书生成及完整下载。
    /// </summary>
    /// <param name="deviceScope">设备相对于当前账号的范围。</param>
    /// <param name="tenantAdmin">是否使用已有租户管理员权限。</param>
    /// <param name="legacyKeys">是否将新证书数据转换为旧版大写开头字段。</param>
    [Theory(Timeout = 30000)]
    [InlineData("same-customer", false, false)]
    [InlineData("other-customer", true, false)]
    [InlineData("same-customer", false, true)]
    public async Task CertificateEndpoints_WhenDeviceIsInScope_PreserveCreationAndDownload(
        string deviceScope, bool tenantAdmin, bool legacyKeys)
    {
        using var certificates = new TestCertificateFiles();
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeded = await SeedDeviceAsync(context, deviceScope);
        var controller = CreateController(scope.ServiceProvider, certificates.Settings, seeded, tenantAdmin);

        var creation = await controller.CreateX509Identity(seeded.DeviceId);
        Assert.Equal((int)ApiCode.Success, creation.Code);
        Assert.Equal(IdentityType.X509Certificate, creation.Data!.IdentityType);
        Assert.NotEqual("original-thumbprint", creation.Data.IdentityId);
        Assert.Null(creation.Data.IdentityValue);

        if (legacyKeys)
        {
            var identity = await context.DeviceIdentities.SingleAsync(item => item.Id == seeded.IdentityId);
            var values = Assert.IsType<Dictionary<string, string>>(JsonObjectSerializer.Deserialize<Dictionary<string, string>>(identity.IdentityValue));
            identity.IdentityValue = JsonObjectSerializer.Serialize(new Dictionary<string, string>
            {
                ["PrivateKey"] = values["privateKey"],
                ["PublicKey"] = values["publicKey"]
            });
            await context.SaveChangesAsync();
        }

        using var downloadScope = _fixture.Services.CreateScope();
        var downloadController = CreateController(downloadScope.ServiceProvider, certificates.Settings, seeded, tenantAdmin);
        var download = Assert.IsType<FileContentResult>(await downloadController.DownloadCertificates(seeded.DeviceId));
        using var stream = new MemoryStream(download.FileContents);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        Assert.Equal(3, archive.Entries.Count);
        var certificatePem = ReadEntry(archive, "client.crt");
        var privateKeyPem = ReadEntry(archive, "client.key");
        Assert.Equal(File.ReadAllText(certificates.Settings.MqttBroker.CACertificateFile), ReadEntry(archive, "ca.crt"));
        using var clientCertificate = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);
        Assert.True(clientCertificate.HasPrivateKey);
        Assert.Equal(creation.Data.IdentityId, clientCertificate.Thumbprint);
    }

    /// <summary>
    /// 创建隔离的测试租户、客户、设备及可下载的旧身份；所有数据属于临时 SQLite fixture。
    /// </summary>
    /// <param name="context">测试数据库上下文。</param>
    /// <param name="deviceScope">目标设备的授权范围。</param>
    /// <returns>当前账号范围及待验证设备、身份标识。</returns>
    private static async Task<SeededDevice> SeedDeviceAsync(ApplicationDbContext context, string deviceScope)
    {
        var tenant = new Tenant { Name = $"certificate-tenant-{Guid.NewGuid():N}" };
        var customer = new Customer { Name = "certificate-customer", Tenant = tenant };
        var targetTenant = deviceScope == "other-tenant"
            ? new Tenant { Name = $"certificate-other-{Guid.NewGuid():N}" }
            : tenant;
        var targetCustomer = deviceScope is "other-tenant" or "other-customer"
            ? new Customer { Name = "certificate-other-customer", Tenant = targetTenant }
            : customer;
        var device = new Device
        {
            Name = "certificate-device",
            DeviceType = DeviceType.Device,
            Tenant = targetTenant,
            Customer = targetCustomer,
            Deleted = deviceScope == "deleted"
        };
        var value = JsonObjectSerializer.Serialize(new Dictionary<string, string>
        {
            ["PrivateKey"] = "original-private-key",
            ["PublicKey"] = "original-public-key"
        });
        var identity = new DeviceIdentity
        {
            Id = Guid.NewGuid(),
            Device = device,
            DeviceId = device.Id,
            IdentityType = IdentityType.X509Certificate,
            IdentityId = "original-thumbprint",
            IdentityValue = value
        };
        device.DeviceIdentity = identity;
        context.Customer.Add(customer);
        context.Device.Add(device);
        await context.SaveChangesAsync();
        return new SeededDevice(tenant.Id, customer.Id, device.Id, identity.Id, value);
    }

    /// <summary>
    /// 使用测试账号的明确声明创建控制器，复用 fixture 依赖但不修改宿主证书配置。
    /// </summary>
    /// <param name="services">当前测试作用域。</param>
    /// <param name="settings">本测试独占的 CA 配置。</param>
    /// <param name="seeded">账号范围及测试对象。</param>
    /// <param name="tenantAdmin">是否添加已有租户管理员角色。</param>
    /// <returns>已设置用户上下文的设备控制器。</returns>
    private static DevicesController CreateController(
        IServiceProvider services, AppSettings settings, SeededDevice seeded, bool tenantAdmin)
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
        var controller = ActivatorUtilities.CreateInstance<DevicesController>(services, Options.Create(settings));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        };
        return controller;
    }

    /// <summary>
    /// 读取已知证书条目，缺失条目时直接失败。
    /// </summary>
    /// <param name="archive">生成的证书包。</param>
    /// <param name="name">预期文件名。</param>
    /// <returns>PEM 正文。</returns>
    private static string ReadEntry(ZipArchive archive, string name)
    {
        using var reader = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(name)).Open());
        return reader.ReadToEnd();
    }

    private sealed record SeededDevice(Guid TenantId, Guid CustomerId, Guid DeviceId, Guid IdentityId, string IdentityValue);

    private sealed class TestCertificateFiles : IDisposable
    {
        private readonly string _root = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
        private readonly string _directory;

        /// <summary>
        /// 创建本测试独占的 CA 文件，供现有证书入口执行真实生成。
        /// </summary>
        public TestCertificateFiles()
        {
            _directory = Path.Combine(_root, $"IoTSharp.DeviceCertificateScopeTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_directory);
            Settings = new AppSettings
            {
                MqttBroker = new MqttBrokerSetting
                {
                    EnableTls = true,
                    DomainName = "http://localhost/",
                    TlsPort = 8883,
                    CACertificateFile = Path.Combine(_directory, "ca.crt"),
                    CAPrivateKeyFile = Path.Combine(_directory, "ca.key")
                }
            };
            try
            {
                using var rsa = RSA.Create(2048);
                var request = new CertificateRequest("CN=IoTSharp certificate scope test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
                using var ca = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
                File.WriteAllText(Settings.MqttBroker.CACertificateFile, ca.ExportCertificatePem());
                File.WriteAllText(Settings.MqttBroker.CAPrivateKeyFile, rsa.ExportRSAPrivateKeyPem());
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public AppSettings Settings { get; }

        /// <summary>
        /// 释放加载的 CA 并仅删除经路径核对的本测试独占目录。
        /// </summary>
        public void Dispose()
        {
            Settings.MqttBroker.CACertificate?.Dispose();
            var directory = Path.GetFullPath(_directory);
            if (!string.Equals(Path.GetDirectoryName(directory), _root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("证书测试临时目录超出所属范围。");
            }
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
