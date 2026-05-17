using CatalogService.Application.Commands;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteProductCommandHandler"/>.
/// </summary>
public class DeleteProductCommandTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_SoftDeletesProduct_WhenFound()
    {
        // Arrange
        using var context = CreateContext();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "To Delete",
            Sku = "DEL-001",
            BasePrice = 10m,
            Currency = "USD",
            CategoryId = Guid.NewGuid(),
            IsActive = true
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(context);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        Assert.True(result);
        var deleted = await context.Products.FindAsync(product.Id);
        Assert.NotNull(deleted);
        Assert.False(deleted!.IsActive);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenProductNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new DeleteProductCommandHandler(context);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
