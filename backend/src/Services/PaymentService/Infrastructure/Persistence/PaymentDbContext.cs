using PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Payment Service.
/// </summary>
public class PaymentDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentDbContext"/> class.
    /// </summary>
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the payment intents DbSet.
    /// </summary>
    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();

    /// <summary>
    /// Gets or sets the payment methods DbSet.
    /// </summary>
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PaymentIntentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
