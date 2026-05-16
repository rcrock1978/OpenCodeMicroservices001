using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Represents a notification record in the system.
/// </summary>
public class Notification
{
    /// <summary>
    /// Gets or sets the unique identifier for the notification.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    public required string RecipientEmail { get; set; }

    /// <summary>
    /// Gets or sets the notification subject.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Gets or sets the notification body.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Email;

    /// <summary>
    /// Gets or sets the current status of the notification.
    /// </summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>
    /// Gets or sets when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the notification was sent.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Gets or sets any error message if sending failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Defines the possible notification types.
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    Push,
    Webhook
}

/// <summary>
/// Defines the possible notification statuses.
/// </summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    RetryScheduled
}

/// <summary>
/// Entity framework configuration for <see cref="Notification"/>.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.RecipientEmail).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).IsRequired();
    }
}
