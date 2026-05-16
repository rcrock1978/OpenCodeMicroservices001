using MassTransit;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;

namespace OrderService.Api.Endpoints;

/// <summary>
/// API endpoints for order management.
/// </summary>
public static class OrderEndpoints
{
    /// <summary>
    /// Maps order-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").WithOpenApi();

        group.MapGet("/", async (OrderDbContext db) =>
            Results.Ok(await db.Orders.AsNoTracking().Include(o => o.Items).ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, OrderDbContext db) =>
            Results.Ok(await db.Orders.AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.TenantId == tenantId)
                .ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, OrderDbContext db) =>
            await db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id) is Order order
                ? Results.Ok(order)
                : Results.NotFound());

        group.MapPost("/", async (CreateOrderRequest request, OrderDbContext db, IPublishEndpoint publishEndpoint) =>
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
            await db.SaveChangesAsync();

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
            });

            return Results.Created($"/api/orders/{order.Id}", order);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, OrderDbContext db, IPublishEndpoint publishEndpoint) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null) return Results.NotFound();
            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                return Results.BadRequest("Cannot cancel shipped or delivered order");

            order.Status = OrderStatus.Cancelled;
            await db.SaveChangesAsync();

            await publishEndpoint.Publish(new OrderCancelledIntegrationEvent
            {
                OrderId = order.Id,
                TenantId = order.TenantId,
                Reason = "Customer requested cancellation"
            });

            return Results.Ok(order);
        });

        return app;
    }
}

/// <summary>
/// Request model for creating an order.
/// </summary>
public record CreateOrderRequest(Guid TenantId, Guid CustomerId, List<CreateOrderItemRequest> Items, decimal ShippingCost, decimal TaxAmount, string Currency, string? ShippingAddress);

/// <summary>
/// Request model for an order line item.
/// </summary>
public record CreateOrderItemRequest(Guid ProductId, Guid ProductVariantId, string ProductName, string Sku, decimal UnitPrice, int Quantity);
