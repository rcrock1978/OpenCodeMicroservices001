using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Billing Service.
/// </summary>
public class BillingDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BillingDbContext"/> class.
    /// </summary>
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the plans DbSet.
    /// </summary>
    public DbSet<Plan> Plans => Set<Plan>();

    /// <summary>
    /// Gets or sets the subscriptions DbSet.
    /// </summary>
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    /// <summary>
    /// Gets or sets the invoices DbSet.
    /// </summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
