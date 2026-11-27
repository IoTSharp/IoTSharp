using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Dtos;

public sealed class SystemEmailSettingsDto
{
    [Required, StringLength(253)]
    public string Host { get; set; }

    [Range(1, 65535)]
    public int Port { get; set; } = 465;

    public bool ImplicitTls { get; set; } = true;

    [Required, StringLength(254)]
    public string Username { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string SenderAddress { get; set; }

    [StringLength(1024)]
    public string Password { get; set; }
}

/// <summary>系统配置的邮件设置回读，仅报告授权码是否已经配置。</summary>
public sealed record SystemEmailSettingsView(string Host, int Port, bool ImplicitTls, string Username, string SenderAddress, bool HasPassword);
