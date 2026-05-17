using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to retrieve all users with their associated tenant.
/// </summary>
public record GetUsersQuery : IRequest<List<User>>;

/// <summary>
/// Handles the <see cref="GetUsersQuery"/>.
/// </summary>
public class GetUsersQueryHandler(IdentityDbContext db) : IRequestHandler<GetUsersQuery, List<User>>
{
    /// <inheritdoc />
    public async Task<List<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await db.Users.AsNoTracking().Include(u => u.Tenant).ToListAsync(cancellationToken);
    }
}
