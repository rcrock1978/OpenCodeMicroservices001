namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when a customer profile is created.
/// </summary>
public record CustomerCreatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(CustomerCreatedIntegrationEvent);

    public Guid CustomerId { get; init; }
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
}

/// <summary>
/// Published when a customer profile is updated.
/// </summary>
public record CustomerUpdatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(CustomerUpdatedIntegrationEvent);

    public Guid CustomerId { get; init; }
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
}
