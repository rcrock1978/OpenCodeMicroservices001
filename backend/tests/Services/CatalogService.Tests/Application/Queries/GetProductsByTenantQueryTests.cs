using CatalogService.Application.Queries;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetProductsByTenantQueryHandler"/>.
/// </summary>
public class GetProductsByTenantQueryTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsActiveProducts_ForSpecificTenant()
    {
        // Arrange
        using var context = CreateContext();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var category = new Category { Id = Guid.NewGuid(), TenantId = tenantA, Name = "Test" };
        var productA = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA,
            Name = "Product A",
            Sku = "SKU-A",
            BasePrice = 10m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = true
        };
        var productAInactive = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA,
            Name = "Product A Inactive",
            Sku = "SKU-A-IN",
            BasePrice = 10m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = false
        };
        var productB = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB,
            Name = "Product B",
            Sku = "SKU-B",
            BasePrice = 20m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = true
        };

        context.Categories.Add(category);
        context.Products.AddRange(productA, productAInactive, productB);
        await context.SaveChangesAsync();

        var handler = new GetProductsByTenantQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductsByTenantQuery(tenantA), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(productA.Id, result[0].Id);
        Assert.Equal("Product A", result[0].Name);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenTenantHasNoActiveProducts()
    {
        // Arrange
        using var context = CreateContext();
        var tenantId = Guid.NewGuid();
        var category = new Category { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Test" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Inactive",
            Sku = "SKU-IN",
            BasePrice = 10m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = false
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new GetProductsByTenantQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductsByTenantQuery(tenantId), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenTenantDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetProductsByTenantQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductsByTenantQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
