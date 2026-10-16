using System;
using System.Security.Claims;
using IoTSharp.Contracts;
using IoTSharp.Services;
using Xunit;

namespace IoTSharp.Test;

public sealed class TenantBlobPathTests
{
    [Fact]
    public void SameFileName_IsIsolatedAcrossCustomers_AndFullPathsRoundTrip()
    {
        var tenant = Guid.NewGuid();
        var first = Principal(tenant, Guid.NewGuid());
        var second = Principal(tenant, Guid.NewGuid());
        Assert.True(TenantBlobPath.TryResolve(first, "images/sensor.png", out var firstPath));
        Assert.True(TenantBlobPath.TryResolve(second, "images/sensor.png", out var secondPath));
        Assert.NotEqual(firstPath, secondPath);
        Assert.True(TenantBlobPath.TryResolve(first, firstPath, out var roundTrip));
        Assert.Equal(firstPath, roundTrip);
        Assert.False(TenantBlobPath.TryResolve(second, firstPath, out _));
    }

    [Theory]
    [InlineData("../other/file")]
    [InlineData("folder/../../file")]
    [InlineData("folder\\..\\file")]
    [InlineData("C:/other/file")]
    public void Traversal_IsRejectedBeforeObjectStorageAccess(string path)
    {
        Assert.False(TenantBlobPath.TryResolve(Principal(Guid.NewGuid(), Guid.NewGuid()), path, out _));
    }

    [Fact]
    public void MissingScope_IsDenied_WhileOperatorRetainsLegacyAccess()
    {
        Assert.False(TenantBlobPath.TryResolve(new ClaimsPrincipal(), "legacy/image.png", out _));
        var admin = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, nameof(UserRole.SystemAdmin))], "test"));
        Assert.True(TenantBlobPath.TryResolve(admin, "legacy/image.png", out var path));
        Assert.Equal("legacy/image.png", path);
    }

    private static ClaimsPrincipal Principal(Guid tenant, Guid customer) => new(new ClaimsIdentity([
        new Claim(IoTSharpClaimTypes.Tenant, tenant.ToString()),
        new Claim(IoTSharpClaimTypes.Customer, customer.ToString())
    ], "test"));
}
