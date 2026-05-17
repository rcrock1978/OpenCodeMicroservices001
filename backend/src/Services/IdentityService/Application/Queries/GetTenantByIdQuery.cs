using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to retrieve a tenant by unique identifier.
/// </summary>
public record GetTenantByIdQuery(Guid Id) : IRequest<Tenant?>;

/// <summary>
/// Handles the <see cref="GetTenantByIdQuery"/>.
/// </summary>
public class GetTenantByIdQueryHandler(IdentityDbContext db) : IRequestHandler<GetTenantByIdQuery, Tenant?>
{
    /// <inheritdoc />
    public async Task<Tenant?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
    }
}
