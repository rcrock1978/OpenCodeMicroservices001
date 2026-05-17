using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public record CreateUserCommand(string Email, string Password, string DisplayName, Guid TenantId, UserRole Role) : IRequest<User>;

/// <summary>
/// Handles the <see cref="CreateUserCommand"/>.
/// </summary>
public class CreateUserCommandHandler(IdentityDbContext db) : IRequestHandler<CreateUserCommand, User>
{
    /// <inheritdoc />
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            TenantId = request.TenantId,
            Role = request.Role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return user;
    }
}
