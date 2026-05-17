using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to authenticate a user and generate a JWT token.
/// </summary>
public record LoginQuery(string Email, string Password) : IRequest<LoginResponse?>;

/// <summary>
/// Response model for successful login.
/// </summary>
public record LoginResponse(string Token, Guid UserId, string Email, string DisplayName, Guid TenantId, string Role);

/// <summary>
/// Handles the <see cref="LoginQuery"/>.
/// </summary>
public class LoginQueryHandler(IdentityDbContext db, IConfiguration config) : IRequestHandler<LoginQuery, LoginResponse?>
{
    /// <inheritdoc />
    public async Task<LoginResponse?> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user, config);
        return new LoginResponse(token, user.Id, user.Email, user.DisplayName, user.TenantId, user.Role.ToString());
    }

    private static string GenerateJwtToken(User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
