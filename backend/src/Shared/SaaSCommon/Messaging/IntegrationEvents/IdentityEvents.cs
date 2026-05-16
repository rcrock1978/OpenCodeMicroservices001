using SaaSCommon.Messaging;

namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when a new tenant is created.
/// </summary>
public record TenantCreatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(TenantCreatedIntegrationEvent);

    public Guid TenantId { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Published when a tenant is updated.
/// </summary>
public record TenantUpdatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(TenantUpdatedIntegrationEvent);

    public Guid TenantId { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
}

/// <summary>
/// Published when a new user registers.
/// </summary>
public record UserRegisteredIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(UserRegisteredIntegrationEvent);

    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string Email { get; init; } = null!;
    public DateTimeOffset RegisteredAt { get; init; }
}
