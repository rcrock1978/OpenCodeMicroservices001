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

    /// <summary>
    /// Gets or sets the order summaries DbSet.
    /// </summary>
    public DbSet<OrderSummary> OrderSummaries => Set<OrderSummary>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new OrderSummaryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
