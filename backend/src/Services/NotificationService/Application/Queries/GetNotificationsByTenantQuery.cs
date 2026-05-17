using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Queries;

/// <summary>
/// Query to retrieve notifications for a specific tenant.
/// </summary>
public record GetNotificationsByTenantQuery(Guid TenantId) : IRequest<List<Notification>>;

/// <summary>
/// Handles the <see cref="GetNotificationsByTenantQuery"/>.
/// </summary>
public class GetNotificationsByTenantHandler(NotificationDbContext db) : IRequestHandler<GetNotificationsByTenantQuery, List<Notification>>
{
    /// <inheritdoc />
    public async Task<List<Notification>> Handle(GetNotificationsByTenantQuery request, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .Where(n => n.TenantId == request.TenantId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
