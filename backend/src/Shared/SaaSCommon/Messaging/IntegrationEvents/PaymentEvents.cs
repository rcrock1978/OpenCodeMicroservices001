namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when a payment is successfully processed.
/// </summary>
public record PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(PaymentProcessedIntegrationEvent);

    public Guid PaymentIntentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset ProcessedAt { get; init; }
}

/// <summary>
/// Published when a payment fails.
/// </summary>
public record PaymentFailedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(PaymentFailedIntegrationEvent);

    public Guid PaymentIntentId { get; init; }
    public Guid OrderId { get; init; }
    public string FailureReason { get; init; } = null!;
}
