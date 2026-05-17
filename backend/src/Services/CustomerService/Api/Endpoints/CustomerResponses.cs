using CustomerService.Domain.Entities;

namespace CustomerService.Api.Endpoints;

/// <summary>
/// Response model for a customer.
/// </summary>
public record CustomerResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateTime CreatedAt,
    List<AddressResponse> Addresses
);

/// <summary>
/// Response model for an address (without back-reference to customer to avoid cycles).
/// </summary>
public record AddressResponse(
    Guid Id,
    Guid CustomerId,
    AddressType Type,
    string Street,
    string City,
    string? State,
    string PostalCode,
    string Country,
    bool IsDefault
);
