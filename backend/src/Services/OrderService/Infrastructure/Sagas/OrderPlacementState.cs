using MassTransit;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// Saga state instance for the order placement flow.
/// </summary>
public class OrderPlacementState : SagaStateMachineInstance
{
    /// <summary>
    /// Gets or sets the saga correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the current state of the saga.
    /// </summary>
    public string CurrentState { get; set; } = null!;

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the total amount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency.
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
