using CatalogService.Application.Commands;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateProductCommandHandler"/>.
/// </summary>
public class UpdateProductCommandTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_UpdatesProduct_WhenFound()
    {
        // Arrange
        using var context = CreateContext();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Old Name",
            Sku = "SKU-OLD",
            BasePrice = 50m,
            Currency = "USD",
            CategoryId = Guid.NewGuid(),
            IsActive = true
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(context);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "New Name",
            Description: "Updated description",
            BasePrice: 75m,
            SalePrice: 60m,
            IsActive: false
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updated = await context.Products.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(75m, updated.BasePrice);
        Assert.Equal(60m, updated.SalePrice);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenProductNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new UpdateProductCommandHandler(context);

        var command = new UpdateProductCommand(
            Id: Guid.NewGuid(),
            Name: "Name",
            Description: null,
            BasePrice: 10m,
            SalePrice: null,
            IsActive: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
