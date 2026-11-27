using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;

namespace IoTSharp.Services;

/// <summary>管理当前实例内有时效、次数限制且只能使用一次的租户申请验证码。</summary>
public sealed class TenantEmailVerification : IDisposable
{
    public const int ResendSeconds = 60;
    public const int ValidMinutes = 10;
    private const int MaxAttempts = 5;
    private const int MaxPending = 400;
    private readonly object _gate = new();
    private readonly Dictionary<string, Challenge> _challenges = new(StringComparer.OrdinalIgnoreCase);
    private readonly FixedWindowRateLimiter _sendLimit = new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 30,
        Window = TimeSpan.FromMinutes(1),
        AutoReplenishment = true
    });

    internal sealed record Challenge(string Code, DateTimeOffset IssuedAt)
    {
        public int Attempts { get; set; }
        public bool Delivered { get; set; }
    }

    /// <summary>为邮箱预留发送机会，发送失败也按相同间隔限流。</summary>
    internal Challenge Reserve(string email)
    {
        var now = DateTimeOffset.UtcNow;
        lock (_gate)
        {
            foreach (var address in _challenges.Where(item => item.Value.IssuedAt.AddMinutes(ValidMinutes) <= now).Select(item => item.Key).ToArray())
                _challenges.Remove(address);

            if (_challenges.TryGetValue(email, out var previous) && previous.IssuedAt.AddSeconds(ResendSeconds) > now)
                return null;

            using var permit = _sendLimit.AttemptAcquire();
            if (!permit.IsAcquired || _challenges.Count >= MaxPending)
                return null;

            var challenge = new Challenge(RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture), now);
            _challenges[email] = challenge;
            return challenge;
        }
    }

    /// <summary>SMTP 接收邮件后启用验证码。</summary>
    internal void MarkDelivered(Challenge challenge)
    {
        lock (_gate)
            challenge.Delivered = true;
    }

    /// <summary>校验并原子消耗验证码，过期、重放和超过尝试次数均拒绝。</summary>
    internal bool TryConsume(string email, string code)
    {
        lock (_gate)
        {
            if (!_challenges.TryGetValue(email, out var challenge) || !challenge.Delivered ||
                challenge.IssuedAt.AddMinutes(ValidMinutes) <= DateTimeOffset.UtcNow || challenge.Attempts >= MaxAttempts)
                return false;

            challenge.Attempts++;
            var matches = CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(challenge.Code), Encoding.ASCII.GetBytes(code));
            if (matches)
                challenge.Attempts = MaxAttempts;
            return matches;
        }
    }

    public void Dispose() => _sendLimit.Dispose();
}
