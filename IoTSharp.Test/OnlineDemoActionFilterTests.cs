#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IoTSharp.Test;

public sealed class OnlineDemoActionFilterTests
{
    [Theory]
    [InlineData("POST", "Alarm", "AckAlarm", UserRole.NormalUser, true, false)]
    [InlineData("POST", "Alarm", "AckAlarm", UserRole.CustomerAdmin, true, true)]
    [InlineData("POST", "Devices", "GetAttributeLatestByKeyNameAndDeviceId", UserRole.NormalUser, true, true)]
    [InlineData("POST", "Rules", "TestTask", UserRole.TenantAdmin, true, false)]
    [InlineData("GET", "Devices", "CreateX509Identity", UserRole.TenantAdmin, true, false)]
    [InlineData("POST", "ReleaseCenter", "Start", UserRole.TenantAdmin, true, false)]
    [InlineData("POST", "ReleaseCenter", "Start", UserRole.TenantAdmin, false, true)]
    public async Task DemoRestriction_PreservesReadbacksAndRecognizesCustomerRole(
        string method, string controller, string action, UserRole role, bool demo, bool expected)
    {
        var tenantId = Guid.NewGuid();
        var path = Path.GetTempFileName();
        try
        {
            var binding = new OnlineDemoSettings.Binding(Guid.NewGuid().ToString(), tenantId, Guid.NewGuid().ToString());
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(binding));
            var settings = new OnlineDemoSettings(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OnlineDemo:Enabled"] = "true",
                ["OnlineDemo:BindingFile"] = path,
                ["OnlineDemo:PasswordFile"] = path,
                ["OnlineDemo:StateDirectory"] = Path.GetDirectoryName(path)
            }).Build());
            var request = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, role.ToString()),
                    new(IoTSharpClaimTypes.Tenant, (demo ? tenantId : Guid.NewGuid()).ToString())
                ], "test"))
            };
            request.Request.Method = method;
            ActionDescriptor descriptor = new ControllerActionDescriptor { ControllerName = controller, ActionName = action };
            var context = new ActionContext(request, new RouteData(), descriptor);
            var execution = new ActionExecutingContext(context, [], new Dictionary<string, object?>(), this);
            var called = false;
            await new OnlineDemoActionFilter(settings).OnActionExecutionAsync(execution, () =>
            {
                called = true;
                return Task.FromResult(new ActionExecutedContext(context, [], this));
            });

            Assert.Equal(expected, called);
            if (!expected) Assert.Equal(403, Assert.IsType<ObjectResult>(execution.Result).StatusCode);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
