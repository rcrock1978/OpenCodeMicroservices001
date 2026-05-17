using CustomerService.Api.Endpoints;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Application.Queries;

/// <summary>
/// Query to retrieve a customer by identifier.
/// </summary>
public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerResponse?>;

/// <summary>
/// Handler for <see cref="GetCustomerByIdQuery"/>.
/// </summary>
public class GetCustomerByIdQueryHandler(CustomerDbContext dbContext)
    : IRequestHandler<GetCustomerByIdQuery, CustomerResponse?>
{
    /// <inheritdoc />
    public async Task<CustomerResponse?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return customer is null ? null : MapToResponse(customer);
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
