using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to retrieve orders for a specific tenant.
/// </summary>
public record GetOrdersByTenantQuery(Guid TenantId) : IRequest<List<Order>>;

/// <summary>
/// Handler for <see cref="GetOrdersByTenantQuery"/>.
/// </summary>
public class GetOrdersByTenantQueryHandler(OrderDbContext db) : IRequestHandler<GetOrdersByTenantQuery, List<Order>>
{
    /// <inheritdoc />
    public async Task<List<Order>> Handle(GetOrdersByTenantQuery request, CancellationToken cancellationToken)
    {
        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);
    }
}
