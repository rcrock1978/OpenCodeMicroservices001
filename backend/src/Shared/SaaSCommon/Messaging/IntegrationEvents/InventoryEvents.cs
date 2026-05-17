namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when inventory is successfully reserved for an order.
/// </summary>
public record InventoryReservedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(InventoryReservedIntegrationEvent);

    public Guid OrderId { get; init; }
    public Dictionary<Guid, int> ReservedQuantities { get; init; } = new();
}

/// <summary>
/// Published when inventory reservation fails.
/// </summary>
public record InventoryReservationFailedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(InventoryReservationFailedIntegrationEvent);

    public Guid OrderId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Published when reserved stock is released.
/// </summary>
public record StockReleasedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(StockReleasedIntegrationEvent);

    public Guid OrderId { get; init; }
    public Dictionary<Guid, int> ReleasedQuantities { get; init; } = new();
}
