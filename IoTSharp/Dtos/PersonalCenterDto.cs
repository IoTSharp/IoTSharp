using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Dtos;

/// <summary>当前用户可编辑的个人资料。</summary>
public sealed class UpdatePersonalProfileDto
{
    [StringLength(80)]
    public string DisplayName { get; set; }

    [StringLength(300)]
    public string Introduction { get; set; }

    [StringLength(32)]
    public string PhoneNumber { get; set; }
}

/// <summary>个人中心显示的资料与组织身份。</summary>
public sealed record PersonalProfileDto(
    string UserName, string Email, string PhoneNumber, string DisplayName, string Introduction,
    string TenantName, string CustomerName, IList<string> Roles, string Avatar, bool HasCustomAvatar);

/// <summary>站内信正文和当前用户的阅读时间。</summary>
public sealed record InboxMessageDto(
    Guid Id, string Title, string Body, string Category, string Sender, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt);

/// <summary>站内信查询结果及全量未读数。</summary>
public sealed record InboxPageDto(IReadOnlyList<InboxMessageDto> Items, long Total, long UnreadCount);

/// <summary>当前用户信箱的未读数。</summary>
public sealed record InboxUnreadCountDto(long UnreadCount);
