using CustomerService.Application.Commands;
using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using MediatR;

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

        group.MapGet("/", async (IMediator mediator) =>
        {
            var customers = await mediator.Send(new GetCustomersQuery());
            return Results.Ok(customers);
        });

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, IMediator mediator) =>
        {
            var customers = await mediator.Send(new GetCustomersByTenantQuery(tenantId));
            return Results.Ok(customers);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            await mediator.Send(new GetCustomerByIdQuery(id)) is { } customer
                ? Results.Ok(customer)
                : Results.NotFound());

        group.MapPost("/", async (CreateCustomerRequest request, IMediator mediator) =>
        {
            var command = new CreateCustomerCommand(
                request.TenantId,
                request.UserId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            var response = await mediator.Send(command);
            return Results.Created($"/api/customers/{response.Id}", response);
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

        group.MapGet("/customer/{customerId:guid}", async (Guid customerId, IMediator mediator) =>
        {
            var addresses = await mediator.Send(new GetAddressesByCustomerQuery(customerId));
            return Results.Ok(addresses);
        });

        group.MapPost("/", async (CreateAddressRequest request, IMediator mediator) =>
        {
            var command = new CreateAddressCommand(
                request.CustomerId,
                request.Type,
                request.Street,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.IsDefault);

            var response = await mediator.Send(command);
            return Results.Created($"/api/addresses/{response.Id}", response);
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
