namespace SaaSCommon.Messaging;

/// <summary>
/// Represents a domain event that can be published across service boundaries.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the type name of the event.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the version of the event schema.
    /// </summary>
    string EventVersion { get; }
}

/// <summary>
/// Abstraction for publishing integration events between microservices.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to the message broker.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to publish.</typeparam>
    /// <param name="event">The event instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}

/// <summary>
/// Base implementation for integration events.
/// </summary>
public abstract class IntegrationEvent : IEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public abstract string EventType { get; }

    /// <inheritdoc />
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <inheritdoc />
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public virtual string EventVersion => "1.0";
}
