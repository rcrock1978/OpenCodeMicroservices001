using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to update an existing tenant.
/// </summary>
public record UpdateTenantCommand(Guid Id, string Name, bool IsActive) : IRequest<Tenant?>;

/// <summary>
/// Handles the <see cref="UpdateTenantCommand"/>.
/// </summary>
public class UpdateTenantCommandHandler(IdentityDbContext db) : IRequestHandler<UpdateTenantCommand, Tenant?>
{
    /// <inheritdoc />
    public async Task<Tenant?> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FindAsync(new object[] { request.Id }, cancellationToken);
        if (tenant is null)
            return null;

        tenant.Name = request.Name;
        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);

        return tenant;
    }
}
