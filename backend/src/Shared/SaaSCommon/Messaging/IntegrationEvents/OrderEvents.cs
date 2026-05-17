namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when an order is placed.
/// </summary>
public record OrderPlacedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderPlacedIntegrationEvent);

    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
}

/// <summary>
/// Represents an order item in integration events.
/// </summary>
public record OrderItemDto(Guid ProductId, Guid? ProductVariantId, string ProductName, string Sku, decimal UnitPrice, int Quantity);

/// <summary>
/// Published when an order payment is confirmed.
/// </summary>
public record OrderPaidIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderPaidIntegrationEvent);

    public Guid OrderId { get; init; }
    public Guid PaymentIntentId { get; init; }
}

/// <summary>
/// Published when an order is cancelled.
/// </summary>
public record OrderCancelledIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderCancelledIntegrationEvent);

    public Guid OrderId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Published when an order is shipped.
/// </summary>
public record OrderShippedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderShippedIntegrationEvent);

    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = null!;
}
