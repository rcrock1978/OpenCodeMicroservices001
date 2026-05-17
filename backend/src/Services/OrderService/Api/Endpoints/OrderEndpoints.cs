using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;

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

        group.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetOrdersQuery())));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetOrdersByTenantQuery(tenantId))));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            await mediator.Send(new GetOrderByIdQuery(id)) is Order order
                ? Results.Ok(order)
                : Results.NotFound());

        group.MapPost("/", async (CreateOrderRequest request, IMediator mediator) =>
        {
            var command = new CreateOrderCommand(
                request.TenantId,
                request.CustomerId,
                request.Items.Select(i => new CreateOrderItemDto(
                    i.ProductId,
                    i.ProductVariantId,
                    i.ProductName,
                    i.Sku,
                    i.UnitPrice,
                    i.Quantity)).ToList(),
                request.ShippingCost,
                request.TaxAmount,
                request.Currency,
                request.ShippingAddress);

            var order = await mediator.Send(command);
            return Results.Created($"/api/orders/{order.Id}", order);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelOrderCommand(id));
            if (!result.Success && result.Order is null)
                return Results.NotFound();
            if (!result.Success)
                return Results.BadRequest(result.Error);

            return Results.Ok(result.Order);
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
