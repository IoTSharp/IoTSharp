using System;
using System.Linq;
using System.Security.Claims;
using IoTSharp.Contracts;

namespace IoTSharp.Services;

/// <summary>把公开文件接口限制到当前租户和客户的对象前缀，系统管理员保留运维访问。</summary>
internal static class TenantBlobPath
{
    /// <summary>获取允许列举的前缀；缺少业务身份时拒绝访问。</summary>
    internal static bool TryGetPrefix(ClaimsPrincipal user, out string prefix)
    {
        prefix = string.Empty;
        if (user.IsInRole(nameof(UserRole.SystemAdmin))) return true;
        if (!Guid.TryParse(user.FindFirstValue(IoTSharpClaimTypes.Tenant), out var tenant) || tenant == Guid.Empty
            || !Guid.TryParse(user.FindFirstValue(IoTSharpClaimTypes.Customer), out var customer) || customer == Guid.Empty) return false;
        prefix = $"tenants/{tenant:N}/customers/{customer:N}/";
        return true;
    }

    /// <summary>接受相对对象路径或本客户完整路径，拒绝跨范围路径和路径穿越。</summary>
    internal static bool TryResolve(ClaimsPrincipal user, string path, out string resolved)
    {
        resolved = string.Empty;
        if (!TryGetPrefix(user, out var prefix) || string.IsNullOrWhiteSpace(path)) return false;
        var relative = path.Replace('\\', '/').TrimStart('/');
        if (relative.Any(char.IsControl) || relative.Contains(':') || relative.Split('/').Any(segment => segment is "." or "..")) return false;
        if (prefix.Length == 0 || relative.StartsWith(prefix, StringComparison.Ordinal))
        {
            resolved = relative;
            return true;
        }
        if (relative.StartsWith("tenants/", StringComparison.Ordinal)) return false;
        resolved = prefix + relative;
        return true;
    }
}
