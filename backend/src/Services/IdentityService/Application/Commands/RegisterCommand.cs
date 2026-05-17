using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to register a new user account.
/// </summary>
public record RegisterCommand(string Email, string Password, string DisplayName, Guid TenantId) : IRequest<RegisterResult>;

/// <summary>
/// Result of a registration command.
/// </summary>
public record RegisterResult(Guid UserId, string Email);

/// <summary>
/// Handles the <see cref="RegisterCommand"/>.
/// </summary>
public class RegisterCommandHandler(IdentityDbContext db) : IRequestHandler<RegisterCommand, RegisterResult>
{
    /// <inheritdoc />
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            TenantId = request.TenantId,
            Role = UserRole.Member
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterResult(user.Id, user.Email);
    }
}
