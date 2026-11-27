using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Dtos;

public class TenantApplicationEmailDto
{
    /// <summary>用于接收验证码的租户管理员邮箱，也是开通后的登录账号。</summary>
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; }
}

public sealed class TenantApplicationDto : TenantApplicationEmailDto
{
    [Required, StringLength(100)]
    public string TenantName { get; set; }

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required, RegularExpression(@"^\+?[0-9]{7,15}$")]
    public string PhoneNumber { get; set; }

    [Required, RegularExpression(@"^[0-9]{6}$")]
    public string VerificationCode { get; set; }
}
