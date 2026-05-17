using CustomerService.Api.Endpoints;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Application.Queries;

/// <summary>
/// Query to retrieve customers by tenant identifier.
/// </summary>
public record GetCustomersByTenantQuery(Guid TenantId) : IRequest<List<CustomerResponse>>;

/// <summary>
/// Handler for <see cref="GetCustomersByTenantQuery"/>.
/// </summary>
public class GetCustomersByTenantQueryHandler(CustomerDbContext dbContext)
    : IRequestHandler<GetCustomersByTenantQuery, List<CustomerResponse>>
{
    /// <inheritdoc />
    public async Task<List<CustomerResponse>> Handle(GetCustomersByTenantQuery request, CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Addresses)
            .Where(c => c.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);

        return customers.Select(MapToResponse).ToList();
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
