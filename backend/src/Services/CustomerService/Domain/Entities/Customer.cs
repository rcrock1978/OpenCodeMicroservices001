using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Domain.Entities;

/// <summary>
/// Represents a customer profile.
/// </summary>
public class Customer
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier from the Identity service.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets when the customer was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for addresses.
    /// </summary>
    public ICollection<Address> Addresses { get; set; } = [];
}

/// <summary>
/// Entity framework configuration for <see cref="Customer"/>.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TenantId, c.Email }).IsUnique();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
    }
}

/// <summary>
/// Represents a customer address.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Navigation property for the customer.
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the address type.
    /// </summary>
    public AddressType Type { get; set; } = AddressType.Shipping;

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public required string Street { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    public required string Country { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default address.
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// Defines the possible address types.
/// </summary>
public enum AddressType
{
    Shipping,
    Billing
}

/// <summary>
/// Entity framework configuration for <see cref="Address"/>.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasOne(a => a.Customer)
               .WithMany(c => c.Addresses)
               .HasForeignKey(a => a.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(a => a.Street).HasMaxLength(300).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(2).IsRequired();
    }
}
