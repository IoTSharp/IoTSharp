using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services;

public interface ITenantVerificationMailSender
{
    Task<bool> SendAsync(string email, string code, CancellationToken cancellationToken);
}

/// <summary>通过系统级 SMTP 配置发送租户申请验证码。</summary>
public sealed class SystemEmailSender(SystemEmailSettings settings) : ITenantVerificationMailSender
{
    /// <summary>向申请人发送验证码。平台尚未配置 SMTP 时返回 false。</summary>
    public async Task<bool> SendAsync(string email, string code, CancellationToken cancellationToken)
    {
        var options = settings.Read();
        if (string.IsNullOrWhiteSpace(options.Password))
            return false;

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("IoTSharp", options.SenderAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "IoTSharp 租户申请验证码";
        message.Body = new TextPart("plain")
        {
            Text = $"您的 IoTSharp 租户申请验证码为 {code}，{TenantEmailVerification.ValidMinutes} 分钟内有效。请勿向他人透露此验证码。"
        };

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(options.Host, options.Port,
            options.ImplicitTls ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, deadline.Token);
        await smtp.AuthenticateAsync(options.Username, options.Password, deadline.Token);
        await smtp.SendAsync(message, deadline.Token);
        return true;
    }
}
