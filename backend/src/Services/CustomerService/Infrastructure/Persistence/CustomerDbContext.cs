using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Customer Service.
/// </summary>
public class CustomerDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerDbContext"/> class.
    /// </summary>
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the customers DbSet.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets or sets the addresses DbSet.
    /// </summary>
    public DbSet<Address> Addresses => Set<Address>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
