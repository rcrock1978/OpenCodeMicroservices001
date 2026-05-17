using MassTransit;
using MediatR;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to cancel an existing order.
/// </summary>
public record CancelOrderCommand(Guid Id) : IRequest<CancelOrderResult>;

/// <summary>
/// Result of a <see cref="CancelOrderCommand"/>.
/// </summary>
public record CancelOrderResult(bool Success, Order? Order = null, string? Error = null);

/// <summary>
/// Handler for <see cref="CancelOrderCommand"/>.
/// </summary>
public class CancelOrderCommandHandler(OrderDbContext db, IPublishEndpoint publishEndpoint) : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    /// <inheritdoc />
    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FindAsync([request.Id], cancellationToken);
        if (order is null)
            return new CancelOrderResult(false, null, "Order not found");

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return new CancelOrderResult(false, order, "Cannot cancel shipped or delivered order");

        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new OrderCancelledIntegrationEvent
        {
            OrderId = order.Id,
            TenantId = order.TenantId,
            Reason = "Customer requested cancellation"
        }, cancellationToken);

        return new CancelOrderResult(true, order);
    }
}
