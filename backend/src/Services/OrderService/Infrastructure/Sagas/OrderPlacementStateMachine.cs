using MassTransit;
using SaaSCommon.Messaging.IntegrationEvents;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// MassTransit saga state machine for the order placement flow:
/// Inventory reserve -> Payment initiation -> Order confirmation.
/// </summary>
public class OrderPlacementStateMachine : MassTransitStateMachine<OrderPlacementState>
{
    /// <summary>
    /// State indicating inventory reservation is pending.
    /// </summary>
    public State InventoryPending { get; private set; } = null!;

    /// <summary>
    /// State indicating payment is pending.
    /// </summary>
    public State PaymentPending { get; private set; } = null!;

    /// <summary>
    /// State indicating the order is completed.
    /// </summary>
    public State Completed { get; private set; } = null!;

    /// <summary>
    /// State indicating the order has failed.
    /// </summary>
    public State Failed { get; private set; } = null!;

    /// <summary>
    /// Event triggered when an order is placed.
    /// </summary>
    public Event<OrderPlacedIntegrationEvent> OrderPlaced { get; private set; } = null!;

    /// <summary>
    /// Event triggered when inventory is reserved.
    /// </summary>
    public Event<InventoryReservedIntegrationEvent> InventoryReserved { get; private set; } = null!;

    /// <summary>
    /// Event triggered when inventory reservation fails.
    /// </summary>
    public Event<InventoryReservationFailedIntegrationEvent> InventoryReservationFailed { get; private set; } = null!;

    /// <summary>
    /// Event triggered when payment is processed.
    /// </summary>
    public Event<PaymentProcessedIntegrationEvent> PaymentProcessed { get; private set; } = null!;

    /// <summary>
    /// Event triggered when payment fails.
    /// </summary>
    public Event<PaymentFailedIntegrationEvent> PaymentFailed { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderPlacementStateMachine"/> class.
    /// </summary>
    public OrderPlacementStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => InventoryReserved, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => InventoryReservationFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentProcessed, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    ctx.Instance.OrderId = ctx.Data.OrderId;
                    ctx.Instance.TenantId = ctx.Data.TenantId;
                    ctx.Instance.TotalAmount = ctx.Data.TotalAmount;
                })
                .TransitionTo(InventoryPending)
                .Publish(ctx => new InventoryReserveCommand
                {
                    OrderId = ctx.Data.OrderId,
                    TenantId = ctx.Data.TenantId,
                    Items = ctx.Data.Items.ToDictionary(i => i.ProductId, i => i.Quantity)
                }));

        During(InventoryPending,
            When(InventoryReserved)
                .TransitionTo(PaymentPending)
                .Publish(ctx => new PaymentInitiateCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Amount = ctx.Instance.TotalAmount
                }),
            When(InventoryReservationFailed)
                .TransitionTo(Failed)
                .Publish(ctx => new OrderCancelCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Reason = ctx.Data.Reason
                }));

        During(PaymentPending,
            When(PaymentProcessed)
                .TransitionTo(Completed)
                .Publish(ctx => new OrderConfirmCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                }),
            When(PaymentFailed)
                .TransitionTo(Failed)
                .Publish(ctx => new InventoryReleaseCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                })
                .Publish(ctx => new OrderCancelCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Reason = ctx.Data.FailureReason
                }));
    }
}

/// <summary>
/// Command to reserve inventory.
/// </summary>
public record InventoryReserveCommand
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public Dictionary<Guid, int> Items { get; init; } = new();
}

/// <summary>
/// Command to initiate payment.
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
/// Command to release inventory.
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
