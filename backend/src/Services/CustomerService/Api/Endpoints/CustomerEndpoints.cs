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
            Results.Ok(await db.Customers.AsNoTracking().Include(c => c.Addresses).ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, CustomerDbContext db) =>
            Results.Ok(await db.Customers.AsNoTracking()
                .Include(c => c.Addresses)
                .Where(c => c.TenantId == tenantId)
                .ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, CustomerDbContext db) =>
            await db.Customers.AsNoTracking().Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id) is Customer customer
                ? Results.Ok(customer)
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

            return Results.Created($"/api/customers/{customer.Id}", customer);
        });

        return app;
    }
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
            Results.Ok(await db.Addresses.AsNoTracking().Where(a => a.CustomerId == customerId).ToListAsync()));

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
            return Results.Created($"/api/addresses/{address.Id}", address);
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
