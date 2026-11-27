using System;

namespace IoTSharp.Data;

/// <summary>发送给指定租户、客户下单个用户的站内消息。</summary>
public sealed class InboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string Category { get; set; }
    public string Sender { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
