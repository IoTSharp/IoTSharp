using IoTSharp.Data;
using Microsoft.AspNetCore.Identity;
using System;

namespace IoTSharp.Services;

internal static class PersonalProfilePresentation
{
    /// <summary>优先返回用户保存的头像，其次返回已有的系统头像。</summary>
    internal static string ResolveAvatar(IdentityUser user, PersonalProfile profile)
    {
        if (profile?.AvatarPng?.Length > 0)
        {
            return $"data:image/png;base64,{Convert.ToBase64String(profile.AvatarPng)}";
        }

        return string.IsNullOrWhiteSpace(user.Email) ? "/api/Account/Avatar" : user.Gravatar();
    }

    /// <summary>已有账号没有显示名时沿用其登录账号名称。</summary>
    internal static string ResolveDisplayName(IdentityUser user, PersonalProfile profile)
        => string.IsNullOrWhiteSpace(profile?.DisplayName) ? user.UserName : profile.DisplayName;
}
