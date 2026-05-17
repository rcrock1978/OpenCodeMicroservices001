using CustomerService.Api.Endpoints;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CustomerService.Application.Commands;

/// <summary>
/// Command to create a new customer.
/// </summary>
public record CreateCustomerCommand(
    Guid TenantId,
    Guid? UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<CustomerResponse>;

/// <summary>
/// Handler for <see cref="CreateCustomerCommand"/>.
/// </summary>
public class CreateCustomerCommandHandler(
    CustomerDbContext dbContext,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<CreateCustomerCommand, CustomerResponse>
{
    /// <inheritdoc />
    public async Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
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

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new CustomerCreatedIntegrationEvent
        {
            TenantId = customer.TenantId,
            CustomerId = customer.Id,
            Email = customer.Email,
            FullName = $"{customer.FirstName} {customer.LastName}"
        }, cancellationToken);

        return MapToResponse(customer);
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
