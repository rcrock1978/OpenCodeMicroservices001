using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using SaaSCommon.Messaging.IntegrationEvents;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to create a new tenant.
/// </summary>
public record CreateTenantCommand(string Name, string Subdomain, string? SubscriptionPlanId) : IRequest<Tenant>;

/// <summary>
/// Handles the <see cref="CreateTenantCommand"/>.
/// </summary>
public class CreateTenantCommandHandler(IdentityDbContext db, IPublishEndpoint publishEndpoint) : IRequestHandler<CreateTenantCommand, Tenant>
{
    /// <inheritdoc />
    public async Task<Tenant> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Subdomain = request.Subdomain,
            SubscriptionPlanId = request.SubscriptionPlanId
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new TenantCreatedIntegrationEvent
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Subdomain,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return tenant;
    }
}
