namespace SaaSCommon.Messaging;

/// <summary>
/// Command to reserve inventory for an order.
/// </summary>
public record InventoryReserveCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public Dictionary<Guid, int> Items { get; init; } = new();
}

/// <summary>
/// Command to initiate payment for an order.
/// </summary>
public record PaymentInitiateCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public decimal Amount { get; init; }
}

/// <summary>
/// Command to cancel an order.
/// </summary>
public record OrderCancelCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Command to release reserved inventory.
/// </summary>
public record InventoryReleaseCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
}

/// <summary>
/// Command to confirm an order.
/// </summary>
public record OrderConfirmCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
}

/// <summary>
/// Command to refund a payment.
/// </summary>
public record RefundPaymentCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
}
