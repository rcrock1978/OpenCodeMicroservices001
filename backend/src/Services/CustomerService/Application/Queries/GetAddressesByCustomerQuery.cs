using CustomerService.Api.Endpoints;
using CustomerService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Application.Queries;

/// <summary>
/// Query to retrieve addresses for a specific customer.
/// </summary>
public record GetAddressesByCustomerQuery(Guid CustomerId) : IRequest<List<AddressResponse>>;

/// <summary>
/// Handler for <see cref="GetAddressesByCustomerQuery"/>.
/// </summary>
public class GetAddressesByCustomerQueryHandler(CustomerDbContext dbContext)
    : IRequestHandler<GetAddressesByCustomerQuery, List<AddressResponse>>
{
    /// <inheritdoc />
    public async Task<List<AddressResponse>> Handle(GetAddressesByCustomerQuery request, CancellationToken cancellationToken)
    {
        var addresses = await dbContext.Addresses
            .AsNoTracking()
            .Where(a => a.CustomerId == request.CustomerId)
            .ToListAsync(cancellationToken);

        return addresses.Select(a => new AddressResponse(
            a.Id,
            a.CustomerId,
            a.Type,
            a.Street,
            a.City,
            a.State,
            a.PostalCode,
            a.Country,
            a.IsDefault)).ToList();
    }
}
