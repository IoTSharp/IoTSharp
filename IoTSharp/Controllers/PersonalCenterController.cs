using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Controllers;

/// <summary>管理当前登录用户的资料、头像和站内信。</summary>
[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/personal-center")]
public sealed class PersonalCenterController(
    ApplicationDbContext context, UserManager<IdentityUser> userManager) : ControllerBase
{
    private const int MaxAvatarBytes = 2 * 1024 * 1024;
    private const int MaxUploadRequestBytes = MaxAvatarBytes + 16 * 1024;
    private const int MaxAvatarSide = 4096;
    private const int MaxAvatarPixels = 16_000_000;
    private const int ThumbnailSide = 192;

    /// <summary>读取登录账号、显示资料和所属组织。</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResult<PersonalProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var profile = await context.PersonalProfiles.FindAsync([user.Id], cancellationToken);
        return new ApiResult<PersonalProfileDto>(await BuildProfileAsync(user, profile, cancellationToken));
    }

    /// <summary>更新当前账号的显示名、简介和联系方式。</summary>
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResult<PersonalProfileDto>>> UpdateProfile(
        [FromBody] UpdatePersonalProfileDto request, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var profile = await GetOrCreateProfileAsync(user.Id, cancellationToken);
        profile.DisplayName = request.DisplayName?.Trim();
        profile.Introduction = request.Introduction?.Trim();
        try
        {
            var phoneNumber = request.PhoneNumber?.Trim();
            if (phoneNumber != null && !string.Equals(user.PhoneNumber ?? string.Empty, phoneNumber, StringComparison.Ordinal))
            {
                // Identity 与个人资料共用上下文，联系方式和显示资料在同一次保存中提交。
                var result = await userManager.SetPhoneNumberAsync(user, string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber);
                if (!result.Succeeded)
                {
                    return Conflict(new ApiResult(ApiCode.InValidData, string.Join(" ", result.Errors.Select(error => error.Description))));
                }
            }
            else
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (DbUpdateException ex) when (ex.Entries.Any(entry => entry.Entity == profile && entry.State == EntityState.Added))
        {
            if (!await context.PersonalProfiles.AsNoTracking().AnyAsync(item => item.UserId == user.Id, cancellationToken))
            {
                throw;
            }
            return ProfileWriteConflict();
        }

        return new ApiResult<PersonalProfileDto>(await BuildProfileAsync(user, profile, cancellationToken));
    }

    /// <summary>验证上传的图片，将像素重新编码为头像缩略图后保存。</summary>
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxAvatarBytes)]
    public async Task<ActionResult<ApiResult<PersonalProfileDto>>> UploadAvatar(
        IFormFile file, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        if (file == null || file.Length is <= 0 or > MaxAvatarBytes)
        {
            return BadRequest(new ApiResult(ApiCode.InValidData, "请选择 2 MiB 以内的头像图片。"));
        }

        using var stream = file.OpenReadStream();
        var avatar = EncodeAvatar(stream);
        if (avatar == null)
        {
            return BadRequest(new ApiResult(ApiCode.InValidData,
                "请上传可解码的静态 PNG、JPEG 或 WebP 图片，边长不超过 4096 像素，总像素不超过 1600 万。"));
        }

        var profile = await GetOrCreateProfileAsync(user.Id, cancellationToken);
        profile.AvatarPng = avatar;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.Entries.Any(entry => entry.Entity == profile && entry.State == EntityState.Added))
        {
            if (!await context.PersonalProfiles.AsNoTracking().AnyAsync(item => item.UserId == user.Id, cancellationToken))
            {
                throw;
            }
            return ProfileWriteConflict();
        }
        return new ApiResult<PersonalProfileDto>(await BuildProfileAsync(user, profile, cancellationToken));
    }

    /// <summary>移除自定义头像并恢复系统生成的头像。</summary>
    [HttpDelete("avatar")]
    public async Task<ActionResult<ApiResult<PersonalProfileDto>>> RemoveAvatar(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var profile = await context.PersonalProfiles.FindAsync([user.Id], cancellationToken);
        if (profile?.AvatarPng != null)
        {
            profile.AvatarPng = null;
            await context.SaveChangesAsync(cancellationToken);
        }
        return new ApiResult<PersonalProfileDto>(await BuildProfileAsync(user, profile, cancellationToken));
    }

    /// <summary>按时间倒序分页查询该用户在当前组织中的站内信。</summary>
    [HttpGet("inbox")]
    public async Task<ActionResult<ApiResult<InboxPageDto>>> GetInbox(
        CancellationToken cancellationToken,
        [FromQuery, Range(1, 100_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] bool unreadOnly = false)
    {
        var messages = await ResolveInboxAsync(cancellationToken);
        if (messages == null)
        {
            return Unauthorized();
        }

        var unread = await messages.LongCountAsync(message => message.ReadAt == null, cancellationToken);
        var filtered = unreadOnly ? messages.Where(message => message.ReadAt == null) : messages;
        var total = unreadOnly ? unread : await filtered.LongCountAsync(cancellationToken);
        var rows = await filtered.OrderByDescending(message => message.CreatedAt).ThenByDescending(message => message.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select(message => new InboxMessageDto(
            message.Id, message.Title, message.Body, message.Category, message.Sender, ToUtc(message.CreatedAt),
            message.ReadAt is DateTime readAt ? ToUtc(readAt) : null)).ToArray();
        return new ApiResult<InboxPageDto>(new InboxPageDto(items, total, unread));
    }

    /// <summary>返回该用户在当前组织中尚未阅读的消息数。</summary>
    [HttpGet("inbox/unread-count")]
    public async Task<ActionResult<ApiResult<InboxUnreadCountDto>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var messages = await ResolveInboxAsync(cancellationToken);
        return messages == null ? Unauthorized() : await CountUnreadAsync(messages, cancellationToken);
    }

    /// <summary>标记一封属于当前用户的站内信为已读。</summary>
    [HttpPut("inbox/{id:guid}/read")]
    public async Task<ActionResult<ApiResult<InboxUnreadCountDto>>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var messages = await ResolveInboxAsync(cancellationToken);
        if (messages == null)
        {
            return Unauthorized();
        }

        var selected = messages.Where(message => message.Id == id);
        var changed = await SetReadAsync(selected, cancellationToken);
        if (changed == 0 && !await selected.AnyAsync(cancellationToken))
        {
            return NotFound(new ApiResult(ApiCode.CantFindObject, "找不到这封站内信。"));
        }
        return await CountUnreadAsync(messages, cancellationToken);
    }

    /// <summary>将当前组织信箱内的所有未读消息标记为已读。</summary>
    [HttpPut("inbox/read-all")]
    public async Task<ActionResult<ApiResult<InboxUnreadCountDto>>> MarkAllRead(CancellationToken cancellationToken)
    {
        var messages = await ResolveInboxAsync(cancellationToken);
        if (messages == null)
        {
            return Unauthorized();
        }
        await SetReadAsync(messages, cancellationToken);
        return await CountUnreadAsync(messages, cancellationToken);
    }

    private async Task<PersonalProfile> GetOrCreateProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await context.PersonalProfiles.FindAsync([userId], cancellationToken);
        if (profile == null)
        {
            profile = new PersonalProfile { UserId = userId };
            context.PersonalProfiles.Add(profile);
        }
        return profile;
    }

    private async Task<PersonalProfileDto> BuildProfileAsync(
        IdentityUser user, PersonalProfile profile, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (tenantId, customerId) = ReadOrganization();
        var organization = await context.Customer.AsNoTracking()
            .Where(customer => customer.Id == customerId && !customer.Deleted
                && customer.Tenant.Id == tenantId && !customer.Tenant.Deleted)
            .Select(customer => new { CustomerName = customer.Name, TenantName = customer.Tenant.Name })
            .SingleOrDefaultAsync(cancellationToken);
        return new PersonalProfileDto(
            user.UserName, user.Email, user.PhoneNumber,
            PersonalProfilePresentation.ResolveDisplayName(user, profile),
            profile?.Introduction ?? string.Empty, organization?.TenantName, organization?.CustomerName,
            roles, PersonalProfilePresentation.ResolveAvatar(user, profile), profile?.AvatarPng?.Length > 0);
    }

    private (Guid TenantId, Guid CustomerId) ReadOrganization()
    {
        Guid.TryParse(User.FindFirstValue(IoTSharpClaimTypes.Tenant), out var tenantId);
        Guid.TryParse(User.FindFirstValue(IoTSharpClaimTypes.Customer), out var customerId);
        return (tenantId, customerId);
    }

    private async Task<IQueryable<InboxMessage>> ResolveInboxAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return null;
        }

        var messages = context.InboxMessages.AsNoTracking();
        var (tenantId, customerId) = ReadOrganization();
        if (tenantId == Guid.Empty || customerId == Guid.Empty)
        {
            return messages.Where(_ => false);
        }

        return messages.Where(message => message.UserId == user.Id
            && message.TenantId == tenantId && message.CustomerId == customerId);
    }

    private static async Task<ApiResult<InboxUnreadCountDto>> CountUnreadAsync(
        IQueryable<InboxMessage> messages, CancellationToken cancellationToken)
        => new(new InboxUnreadCountDto(await messages.LongCountAsync(message => message.ReadAt == null, cancellationToken)));

    private static Task<int> SetReadAsync(IQueryable<InboxMessage> messages, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return messages.Where(message => message.ReadAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(message => message.ReadAt, now), cancellationToken);
    }

    private ConflictObjectResult ProfileWriteConflict()
        => Conflict(new ApiResult(ApiCode.InValidData, "个人资料刚刚更新，请刷新后重试。"));

    private static DateTimeOffset ToUtc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    /// <summary>以解码器识别的实际格式校验图片，并去除原文件及其元数据。</summary>
    internal static byte[] EncodeAvatar(Stream stream)
    {
        using var codec = SKCodec.Create(stream);
        if (codec == null || codec.EncodedFormat is not (SKEncodedImageFormat.Png or SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Webp)
            || codec.Info.Width is <= 0 or > MaxAvatarSide || codec.Info.Height is <= 0 or > MaxAvatarSide
            || codec.Info.Width * codec.Info.Height > MaxAvatarPixels || codec.FrameCount > 1)
        {
            return null;
        }

        var sourceInfo = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var source = new SKBitmap(sourceInfo);
        if (codec.GetPixels(sourceInfo, source.GetPixels()) != SKCodecResult.Success)
        {
            return null;
        }

        var ratio = Math.Min(1d, ThumbnailSide / (double)Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * ratio));
        var height = Math.Max(1, (int)Math.Round(source.Height * ratio));
        using var thumbnail = source.Resize(sourceInfo.WithSize(width, height), new SKSamplingOptions(SKFilterMode.Linear));
        if (thumbnail == null)
        {
            return null;
        }

        using var oriented = NormalizeOrientation(thumbnail, codec.EncodedOrigin);
        using var encoded = (oriented ?? thumbnail).Encode(SKEncodedImageFormat.Png, 100);
        return encoded?.ToArray();
    }

    private static SKBitmap NormalizeOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
        {
            return null;
        }

        var width = source.Width;
        var height = source.Height;
        var (matrix, outputWidth, outputHeight) = origin switch
        {
            SKEncodedOrigin.TopRight => (SKMatrix.CreateScale(-1, 1, width / 2f, 0), width, height),
            SKEncodedOrigin.BottomRight => (SKMatrix.CreateScale(-1, -1, width / 2f, height / 2f), width, height),
            SKEncodedOrigin.BottomLeft => (SKMatrix.CreateScale(1, -1, 0, height / 2f), width, height),
            SKEncodedOrigin.LeftTop => (new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1), height, width),
            SKEncodedOrigin.RightTop => (new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1), height, width),
            SKEncodedOrigin.RightBottom => (new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1), height, width),
            SKEncodedOrigin.LeftBottom => (new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1), height, width),
            _ => (SKMatrix.Identity, width, height)
        };
        var result = new SKBitmap(source.Info.WithSize(outputWidth, outputHeight));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        return result;
    }
}
