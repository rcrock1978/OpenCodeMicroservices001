using IdentityService.Domain.Entities;

namespace IdentityService.Api.Endpoints;

/// <summary>
/// Response model for a user (without back-reference cycles).
/// </summary>
public record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    Guid TenantId,
    TenantSummaryResponse? Tenant,
    UserRole Role,
    DateTime CreatedAt,
    bool IsActive
);

/// <summary>
/// Summary response model for a tenant (without nested users to avoid cycles).
/// </summary>
public record TenantSummaryResponse(
    Guid Id,
    string Name,
    string Subdomain,
    string? SubscriptionPlanId,
    DateTime CreatedAt,
    bool IsActive
);
