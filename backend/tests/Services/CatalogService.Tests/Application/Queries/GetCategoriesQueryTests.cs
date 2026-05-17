using CatalogService.Application.Queries;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetCategoriesQueryHandler"/>.
/// </summary>
public class GetCategoriesQueryTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsAllCategories()
    {
        // Arrange
        using var context = CreateContext();
        var category1 = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Electronics" };
        var category2 = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Books" };

        context.Categories.AddRange(category1, category2);
        await context.SaveChangesAsync();

        var handler = new GetCategoriesQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Electronics");
        Assert.Contains(result, c => c.Name == "Books");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetCategoriesQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
