using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Operations;

public class Notification : MustHaveTenantEntityBase
{
    public Guid? UserId { get; set; }
    public string Channel { get; set; } = default!; // Email, SMS, WhatsApp
    public string Recipient { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string Status { get; set; } = "Queued"; // Queued, Sent, Failed
    public DateTime? SentAt { get; set; }

    private Notification() { } // EF Core

    public Notification(Guid tenantId, Guid? userId, string channel, string recipient, string title, string body)
    {
        TenantId = tenantId;
        UserId = userId;
        Channel = channel;
        Recipient = recipient;
        Title = title;
        Body = body;
    }

    public void MarkSent()
    {
        Status = "Sent";
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = "Failed";
    }
}
