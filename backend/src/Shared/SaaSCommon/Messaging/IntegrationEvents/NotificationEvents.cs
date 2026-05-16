namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when a notification is sent.
/// </summary>
public record NotificationSentIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(NotificationSentIntegrationEvent);

    public Guid NotificationId { get; init; }
    public Guid TenantId { get; init; }
    public string Recipient { get; init; } = null!;
    public string Channel { get; init; } = null!;
    public string TemplateKey { get; init; } = null!;
}
