#nullable enable
using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using IoTSharp.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IoTSharp.Services;

public sealed class OnlineDemoSettings
{
    internal const string Section = "OnlineDemo";
    internal const int ResetMinutes = 60;
    internal Binding? AccountBinding { get; }
    internal string StateDirectory { get; } = "";
    private string PasswordFile { get; } = "";

    public OnlineDemoSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection(Section);
        if (!section.GetValue<bool>("Enabled")) return;

        StateDirectory = RequiredPath(section, "StateDirectory");
        PasswordFile = RequiredPath(section, "PasswordFile");
        AccountBinding = JsonSerializer.Deserialize<Binding>(File.ReadAllBytes(RequiredPath(section, "BindingFile")));
        if (AccountBinding is null || AccountBinding.DemoTenantId == Guid.Empty
            || !Guid.TryParse(AccountBinding.DemoUserId, out var demoUserId) || demoUserId == Guid.Empty
            || !Guid.TryParse(AccountBinding.AdminUserId, out var adminUserId) || adminUserId == Guid.Empty
            || demoUserId == adminUserId)
            throw new InvalidOperationException("演示账号绑定无效。");
    }

    internal bool IsDemo(ClaimsPrincipal user) => AccountBinding is { } binding
        && (user.FindFirstValue(ClaimTypes.NameIdentifier) == binding.DemoUserId
            || Guid.TryParse(user.FindFirstValue(IoTSharpClaimTypes.Tenant), out var tenant) && tenant == binding.DemoTenantId);

    internal static bool IsScopedCustomer(ControllerBase controller) =>
        controller.HttpContext?.RequestServices?.GetService(typeof(OnlineDemoSettings)) is OnlineDemoSettings settings
        && settings.IsDemo(controller.User) && !controller.User.IsInRole(nameof(UserRole.TenantAdmin));

    internal string ReadPassword()
    {
        using var json = JsonDocument.Parse(File.ReadAllBytes(PasswordFile));
        var password = json.RootElement.GetProperty("demoPassword").GetString();
        if (password is null || password.Length is < 6 or > 256)
            throw new InvalidOperationException("演示密码文件格式无效。");
        return password;
    }

    private static string RequiredPath(IConfiguration section, string key) => section[key] is { } path && Path.IsPathFullyQualified(path)
        ? path
        : throw new InvalidOperationException($"OnlineDemo:{key} 需要绝对路径。");

    internal sealed record Binding(string DemoUserId, Guid DemoTenantId, string AdminUserId);
}
