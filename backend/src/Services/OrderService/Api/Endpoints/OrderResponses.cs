using OrderService.Domain.Entities;

namespace OrderService.Api.Endpoints;

/// <summary>
/// Response model for an order.
/// </summary>
public record OrderResponse(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    string OrderNumber,
    OrderStatus Status,
    decimal Subtotal,
    decimal ShippingCost,
    decimal TaxAmount,
    decimal Total,
    string Currency,
    string? ShippingAddress,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);

/// <summary>
/// Response model for an order line item (without back-reference to order to avoid cycles).
/// </summary>
public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
