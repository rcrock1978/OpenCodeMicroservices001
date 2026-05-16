using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Notification Service.
/// </summary>
public class NotificationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDbContext"/> class.
    /// </summary>
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the notifications DbSet.
    /// </summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Gets or sets the templates DbSet.
    /// </summary>
    public DbSet<Template> Templates => Set<Template>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
