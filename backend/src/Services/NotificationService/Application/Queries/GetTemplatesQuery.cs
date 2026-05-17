using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Queries;

/// <summary>
/// Query to retrieve all notification templates.
/// </summary>
public record GetTemplatesQuery : IRequest<List<Template>>;

/// <summary>
/// Handles the <see cref="GetTemplatesQuery"/>.
/// </summary>
public class GetTemplatesHandler(NotificationDbContext db) : IRequestHandler<GetTemplatesQuery, List<Template>>
{
    /// <inheritdoc />
    public async Task<List<Template>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await db.Templates
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
