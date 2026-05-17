using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to retrieve a user by unique identifier with their associated tenant.
/// </summary>
public record GetUserByIdQuery(Guid Id) : IRequest<User?>;

/// <summary>
/// Handles the <see cref="GetUserByIdQuery"/>.
/// </summary>
public class GetUserByIdQueryHandler(IdentityDbContext db) : IRequestHandler<GetUserByIdQuery, User?>
{
    /// <inheritdoc />
    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Users.AsNoTracking()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
    }
}
