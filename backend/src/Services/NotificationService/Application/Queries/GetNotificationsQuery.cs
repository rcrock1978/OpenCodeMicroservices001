using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Queries;

/// <summary>
/// Query to retrieve all notifications ordered by creation date descending.
/// </summary>
public record GetNotificationsQuery : IRequest<List<Notification>>;

/// <summary>
/// Handles the <see cref="GetNotificationsQuery"/>.
/// </summary>
public class GetNotificationsHandler(NotificationDbContext db) : IRequestHandler<GetNotificationsQuery, List<Notification>>
{
    /// <inheritdoc />
    public async Task<List<Notification>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
