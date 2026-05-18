using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to retrieve a tenant by its unique subdomain.
/// </summary>
public record GetTenantBySubdomainQuery(string Subdomain) : IRequest<Tenant?>;

/// <summary>
/// Handles the <see cref="GetTenantBySubdomainQuery"/>.
/// </summary>
public class GetTenantBySubdomainQueryHandler(IdentityDbContext db) : IRequestHandler<GetTenantBySubdomainQuery, Tenant?>
{
    /// <inheritdoc />
    public async Task<Tenant?> Handle(GetTenantBySubdomainQuery request, CancellationToken cancellationToken)
    {
        return await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Subdomain == request.Subdomain, cancellationToken);
    }
}
