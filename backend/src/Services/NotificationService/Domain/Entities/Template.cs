using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Represents a notification template.
/// </summary>
public class Template
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the template key (e.g., "order-confirmation").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the template subject.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Gets or sets the HTML body template.
    /// </summary>
    public string? BodyHtml { get; set; }

    /// <summary>
    /// Gets or sets the plain text body template.
    /// </summary>
    public string? BodyText { get; set; }

    /// <summary>
    /// Gets or sets the notification channel.
    /// </summary>
    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;

    /// <summary>
    /// Gets or sets when the template was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible notification channels.
/// </summary>
public enum NotificationChannel
{
    Email,
    Sms,
    Push
}

/// <summary>
/// Entity framework configuration for <see cref="Template"/>.
/// </summary>
public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.TenantId, t.Key }).IsUnique();
        builder.Property(t => t.Key).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Subject).HasMaxLength(500).IsRequired();
    }
}
