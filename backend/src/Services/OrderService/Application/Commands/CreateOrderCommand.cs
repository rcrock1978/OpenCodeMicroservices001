using MassTransit;
using MediatR;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to create a new order.
/// </summary>
public record CreateOrderCommand(
    Guid TenantId,
    Guid CustomerId,
    List<CreateOrderItemDto> Items,
    decimal ShippingCost,
    decimal TaxAmount,
    string Currency,
    string? ShippingAddress) : IRequest<Order>;

/// <summary>
/// DTO for an order item within <see cref="CreateOrderCommand"/>.
/// </summary>
public record CreateOrderItemDto(
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity);

/// <summary>
/// Handler for <see cref="CreateOrderCommand"/>.
/// </summary>
public class CreateOrderCommandHandler(OrderDbContext db, IPublishEndpoint publishEndpoint) : IRequestHandler<CreateOrderCommand, Order>
{
    /// <inheritdoc />
    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            OrderNumber = $"ORD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Status = OrderStatus.Pending,
            Subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            ShippingCost = request.ShippingCost,
            TaxAmount = request.TaxAmount,
            Total = request.Items.Sum(i => i.UnitPrice * i.Quantity) + request.ShippingCost + request.TaxAmount,
            Currency = request.Currency,
            ShippingAddress = request.ShippingAddress
        };

        foreach (var item in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                Sku = item.Sku,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.UnitPrice * item.Quantity
            });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new OrderPlacedIntegrationEvent
        {
            OrderId = order.Id,
            TenantId = order.TenantId,
            CustomerId = order.CustomerId,
            TotalAmount = order.Total,
            Items = order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductVariantId,
                i.ProductName,
                i.Sku,
                i.UnitPrice,
                i.Quantity)).ToList()
        }, cancellationToken);

        return order;
    }
}
