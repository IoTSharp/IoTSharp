#nullable enable
using System;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IoTSharp.Services;

/// <summary>限制公开演示账号可调用的操作，阻止设备接入、脚本运行和外部投递。</summary>
internal sealed class OnlineDemoActionFilter(OnlineDemoSettings settings) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!settings.IsDemo(context.HttpContext.User)
            || context.ActionDescriptor is ControllerActionDescriptor action && IsAllowed(context.HttpContext, action))
        {
            await next();
            return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden, Title = "该操作在公开演示租户中不可用。"
        }) { StatusCode = StatusCodes.Status403Forbidden };
    }

    private static bool IsAllowed(HttpContext context, ControllerActionDescriptor action)
    {
        var method = context.Request.Method;
        var route = (action.ControllerName, action.ActionName);
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            return route switch
            {
                ("Devices", "CreateX509Identity" or "DownloadCertificates" or "SessionStatus" or "ClientStatus" or "SessionsCount" or "GetIdentity") => false,
                ("Rules", "GetFlowOperations") => false,
                ("DashBoard", "Trend") => context.User.IsInRole(nameof(UserRole.TenantAdmin)),
                ("Metrics" or "HealthChecks" or "DynamicFormRemoteApi", _) => false,
                (_, var name) when name.StartsWith("Delete", StringComparison.Ordinal)
                    || name.StartsWith("Reset", StringComparison.Ordinal) => false,
                _ => true
            };
        }

        return HttpMethods.IsPost(method) && route switch
        {
            ("Account", "Login" or "RefreshToken" or "Logout") => true,
            ("Alarm", "List" or "Originators") => true,
            ("Alarm", "AckAlarm" or "ClearAlarm") => context.User.IsInRole(nameof(UserRole.CustomerAdmin)),
            ("Customers", "GetCustomers" or "GetAllCustomers") => true,
            ("Rules", "Index" or "FlowEvents" or "Executors") => true,
            ("Devices", "GetAttributeLatestByKeyNameAndDeviceId" or "GetTelemetryData") => true,
            _ => false
        };
    }
}
