using CatalogService.Application.Commands;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace CatalogService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateProductCommandHandler"/>.
/// </summary>
public class CreateProductCommandTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesProduct_PublishesEvent_AndReturnsResponse()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Gadgets" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateProductCommandHandler(context, fakePublisher);

        var command = new CreateProductCommand(
            TenantId: category.TenantId,
            Name: "Smart Watch",
            Description: "A fancy watch",
            Sku: "SW-001",
            BasePrice: 199.99m,
            SalePrice: 149.99m,
            Currency: "USD",
            CategoryId: category.Id
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smart Watch", result.Name);
        Assert.Equal("SW-001", result.Sku);
        Assert.Equal(199.99m, result.BasePrice);
        Assert.Equal(149.99m, result.SalePrice);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.True(result.IsActive);

        var dbProduct = await context.Products.FindAsync(result.Id);
        Assert.NotNull(dbProduct);
        Assert.Equal("Smart Watch", dbProduct!.Name);

        Assert.Single(fakePublisher.PublishedMessages);
        var publishedEvent = Assert.IsType<ProductCreatedIntegrationEvent>(fakePublisher.PublishedMessages[0]);
        Assert.Equal(result.Id, publishedEvent.ProductId);
        Assert.Equal(category.TenantId, publishedEvent.TenantId);
        Assert.Equal("Smart Watch", publishedEvent.Name);
        Assert.Equal("SW-001", publishedEvent.Sku);
        Assert.Equal(199.99m, publishedEvent.Price);
    }

    [Fact]
    public async Task Handle_CreatesProduct_WithNullDescriptionAndSalePrice()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), Name = "Misc" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateProductCommandHandler(context, fakePublisher);

        var command = new CreateProductCommand(
            TenantId: category.TenantId,
            Name: "Sticker",
            Description: null,
            Sku: "STK-001",
            BasePrice: 1.00m,
            SalePrice: null,
            Currency: "USD",
            CategoryId: category.Id
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Description);
        Assert.Null(result.SalePrice);
        Assert.NotNull(await context.Products.FindAsync(result.Id));
    }
}
