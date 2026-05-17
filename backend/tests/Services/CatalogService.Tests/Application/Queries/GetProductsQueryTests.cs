using CatalogService.Application.Queries;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetProductsQueryHandler"/>.
/// </summary>
public class GetProductsQueryTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsAllProducts_WithVariantsAndCategory()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Electronics" };
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = category.TenantId,
            Name = "Laptop",
            Sku = "LAP-001",
            BasePrice = 999.99m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = true
        };
        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = category.TenantId,
            Name = "Mouse",
            Sku = "MOU-001",
            BasePrice = 29.99m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = true
        };
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Name = "16GB RAM",
            Sku = "LAP-001-16GB"
        };

        context.Categories.Add(category);
        context.Products.AddRange(product1, product2);
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        var laptop = result.Single(p => p.Id == product1.Id);
        Assert.Equal("Laptop", laptop.Name);
        Assert.Equal("LAP-001", laptop.Sku);
        Assert.Equal(999.99m, laptop.BasePrice);
        Assert.NotNull(laptop.Category);
        Assert.Equal(category.Id, laptop.Category!.Id);
        Assert.Single(laptop.Variants);
        Assert.Equal("16GB RAM", laptop.Variants[0].Name);

        var mouse = result.Single(p => p.Id == product2.Id);
        Assert.Empty(mouse.Variants);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoProductsExist()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetProductsQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
