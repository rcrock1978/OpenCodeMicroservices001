using MassTransit;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CustomerService.Api.Endpoints;

/// <summary>
/// API endpoints for customer management.
/// </summary>
public static class CustomerEndpoints
{
    /// <summary>
    /// Maps customer-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").WithOpenApi();

        group.MapGet("/", async (CustomerDbContext db) =>
        {
            var customers = await db.Customers.AsNoTracking().Include(c => c.Addresses).ToListAsync();
            return Results.Ok(customers.Select(MapToResponse));
        });

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, CustomerDbContext db) =>
        {
            var customers = await db.Customers.AsNoTracking()
                .Include(c => c.Addresses)
                .Where(c => c.TenantId == tenantId)
                .ToListAsync();
            return Results.Ok(customers.Select(MapToResponse));
        });

        group.MapGet("/{id:guid}", async (Guid id, CustomerDbContext db) =>
            await db.Customers.AsNoTracking().Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id) is Customer customer
                ? Results.Ok(MapToResponse(customer))
                : Results.NotFound());

        group.MapPost("/", async (CreateCustomerRequest request, CustomerDbContext db, IPublishEndpoint publishEndpoint) =>
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserId = request.UserId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            await publishEndpoint.Publish(new CustomerCreatedIntegrationEvent
            {
                CustomerId = customer.Id,
                TenantId = customer.TenantId,
                Email = customer.Email,
                FullName = $"{customer.FirstName} {customer.LastName}"
            });

            return Results.Created($"/api/customers/{customer.Id}", MapToResponse(customer));
        });

        return app;
    }

    private static CustomerResponse MapToResponse(Customer customer) =>
        new(
            customer.Id,
            customer.TenantId,
            customer.UserId,
            customer.Email,
            customer.FirstName,
            customer.LastName,
            customer.PhoneNumber,
            customer.CreatedAt,
            customer.Addresses.Select(a => new AddressResponse(
                a.Id,
                a.CustomerId,
                a.Type,
                a.Street,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                a.IsDefault)).ToList()
        );
}

/// <summary>
/// API endpoints for address management.
/// </summary>
public static class AddressEndpoints
{
    /// <summary>
    /// Maps address-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/addresses").WithTags("Addresses").WithOpenApi();

        group.MapGet("/customer/{customerId:guid}", async (Guid customerId, CustomerDbContext db) =>
        {
            var addresses = await db.Addresses.AsNoTracking().Where(a => a.CustomerId == customerId).ToListAsync();
            return Results.Ok(addresses.Select(a => new AddressResponse(
                a.Id, a.CustomerId, a.Type, a.Street, a.City, a.State, a.PostalCode, a.Country, a.IsDefault)));
        });

        group.MapPost("/", async (CreateAddressRequest request, CustomerDbContext db) =>
        {
            var address = new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Type = request.Type,
                Street = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault
            };
            db.Addresses.Add(address);
            await db.SaveChangesAsync();
            return Results.Created($"/api/addresses/{address.Id}", new AddressResponse(
                address.Id, address.CustomerId, address.Type, address.Street, address.City, address.State, address.PostalCode, address.Country, address.IsDefault));
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a customer.
/// </summary>
public record CreateCustomerRequest(Guid TenantId, Guid? UserId, string Email, string FirstName, string LastName, string? PhoneNumber);

/// <summary>
/// Request model for creating an address.
/// </summary>
public record CreateAddressRequest(Guid CustomerId, AddressType Type, string Street, string City, string? State, string PostalCode, string Country, bool IsDefault);
