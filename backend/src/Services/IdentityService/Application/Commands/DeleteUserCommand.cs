using IdentityService.Infrastructure.Persistence;
using MediatR;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to delete a user by unique identifier.
/// </summary>
public record DeleteUserCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handles the <see cref="DeleteUserCommand"/>.
/// </summary>
public class DeleteUserCommandHandler(IdentityDbContext db) : IRequestHandler<DeleteUserCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync(new object[] { request.Id }, cancellationToken);
        if (user is null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
