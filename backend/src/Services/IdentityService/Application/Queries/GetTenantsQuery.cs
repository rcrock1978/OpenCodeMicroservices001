using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to retrieve all tenants.
/// </summary>
public record GetTenantsQuery : IRequest<List<Tenant>>;

/// <summary>
/// Handles the <see cref="GetTenantsQuery"/>.
/// </summary>
public class GetTenantsQueryHandler(IdentityDbContext db) : IRequestHandler<GetTenantsQuery, List<Tenant>>
{
    /// <inheritdoc />
    public async Task<List<Tenant>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        return await db.Tenants.AsNoTracking().ToListAsync(cancellationToken);
    }
}
