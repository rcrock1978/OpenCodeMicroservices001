using MassTransit;
using SaaSCommon.Messaging;
using SaaSCommon.Messaging.IntegrationEvents;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// MassTransit saga state machine for the order cancellation flow:
/// Payment refund -> Inventory release -> Notification.
/// </summary>
public class OrderCancellationStateMachine : MassTransitStateMachine<OrderCancellationState>
{
    /// <summary>
    /// State indicating refund is pending.
    /// </summary>
    public State RefundPending { get; private set; } = null!;

    /// <summary>
    /// State indicating the cancellation is completed.
    /// </summary>
    public State Completed { get; private set; } = null!;

    /// <summary>
    /// Event triggered when an order is cancelled.
    /// </summary>
    public Event<OrderCancelledIntegrationEvent> OrderCancelled { get; private set; } = null!;

    /// <summary>
    /// Event triggered when a payment is refunded.
    /// </summary>
    public Event<PaymentRefundedIntegrationEvent> PaymentRefunded { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCancellationStateMachine"/> class.
    /// </summary>
    public OrderCancellationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCancelled, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentRefunded, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Instance.OrderId = ctx.Data.OrderId;
                    ctx.Instance.TenantId = ctx.Data.TenantId;
                    ctx.Instance.RequiresRefund = true;
                })
                .IfElse(
                    ctx => ctx.Instance.RequiresRefund,
                    then => then
                        .TransitionTo(RefundPending)
                        .Publish(ctx => new RefundPaymentCommand
                        {
                            OrderId = ctx.Data.OrderId,
                            TenantId = ctx.Data.TenantId
                        }),
                    elseBranch => elseBranch
                        .Publish(ctx => new InventoryReleaseCommand
                        {
                            OrderId = ctx.Data.OrderId,
                            TenantId = ctx.Data.TenantId
                        })
                        .TransitionTo(Completed)));

        During(RefundPending,
            When(PaymentRefunded)
                .Publish(ctx => new InventoryReleaseCommand
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                })
                .TransitionTo(Completed));
    }
}

/// <summary>
/// Event published when a payment is refunded.
/// </summary>
public record PaymentRefundedIntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid TenantId { get; init; }
    public Guid PaymentIntentId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset RefundedAt { get; init; }
}


