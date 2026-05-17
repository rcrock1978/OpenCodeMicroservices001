using CatalogService.Application.Commands;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCategoryCommandHandler"/>.
/// </summary>
public class CreateCategoryCommandTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesCategory_AndReturnsResponse()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreateCategoryCommandHandler(context);

        var tenantId = Guid.NewGuid();
        var command = new CreateCategoryCommand(
            TenantId: tenantId,
            Name: "Furniture",
            ParentCategoryId: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Furniture", result.Name);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Null(result.ParentCategoryId);
        Assert.True(result.IsActive);

        var dbCategory = await context.Categories.FindAsync(result.Id);
        Assert.NotNull(dbCategory);
        Assert.Equal("Furniture", dbCategory!.Name);
    }

    [Fact]
    public async Task Handle_CreatesCategory_WithParentCategory()
    {
        // Arrange
        using var context = CreateContext();
        var parentCategory = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Parent" };
        context.Categories.Add(parentCategory);
        await context.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(context);

        var command = new CreateCategoryCommand(
            TenantId: parentCategory.TenantId,
            Name: "Child",
            ParentCategoryId: parentCategory.Id
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(parentCategory.Id, result.ParentCategoryId);
        var dbCategory = await context.Categories.FindAsync(result.Id);
        Assert.NotNull(dbCategory);
        Assert.Equal(parentCategory.Id, dbCategory!.ParentCategoryId);
    }
}
