using IoTSharp.Contracts;
using IoTSharp.Dtos;
using IoTSharp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTSharp.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
[Route("api/[controller]/email")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SystemSettingsController(SystemEmailSettings settings) : ControllerBase
{
    /// <summary>读取平台 SMTP 连接设置及授权码配置状态。</summary>
    [HttpGet]
    public ApiResult<SystemEmailSettingsView> GetEmail() => new(ApiCode.Success, "OK", settings.GetView());

    /// <summary>保存平台 SMTP 配置，立即用于后续发送。</summary>
    [HttpPut]
    public ApiResult<SystemEmailSettingsView> SetEmail(SystemEmailSettingsDto request) => settings.Save(request)
        ? GetEmail()
        : new(ApiCode.InValidData, "请检查 SMTP 服务器地址并填写 SMTP 授权码。", null);
}
