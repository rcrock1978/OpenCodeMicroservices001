using CatalogService.Application.Queries;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetProductByIdQueryHandler"/>.
/// </summary>
public class GetProductByIdQueryTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsProduct_WhenFound()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Books" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = category.TenantId,
            Name = "C# in Depth",
            Sku = "BK-001",
            BasePrice = 45.00m,
            Currency = "USD",
            CategoryId = category.Id,
            IsActive = true
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal("C# in Depth", result.Name);
        Assert.NotNull(result.Category);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetProductByIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
