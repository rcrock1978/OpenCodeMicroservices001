using CustomerService.Api.Endpoints;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using MediatR;

namespace CustomerService.Application.Commands;

/// <summary>
/// Command to create a new address.
/// </summary>
public record CreateAddressCommand(
    Guid CustomerId,
    AddressType Type,
    string Street,
    string City,
    string? State,
    string PostalCode,
    string Country,
    bool IsDefault
) : IRequest<AddressResponse>;

/// <summary>
/// Handler for <see cref="CreateAddressCommand"/>.
/// </summary>
public class CreateAddressCommandHandler(CustomerDbContext dbContext)
    : IRequestHandler<CreateAddressCommand, AddressResponse>
{
    /// <inheritdoc />
    public async Task<AddressResponse> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
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

        dbContext.Addresses.Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AddressResponse(
            address.Id,
            address.CustomerId,
            address.Type,
            address.Street,
            address.City,
            address.State,
            address.PostalCode,
            address.Country,
            address.IsDefault);
    }
}
