using InventoryService.Application.Queries;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Application.Queries;

/// <summary>
/// Unit tests for the <see cref="GetStockItemByIdQueryHandler"/>.
/// </summary>
public class GetStockItemByIdQueryTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnsStockItem()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithExistingId_ReturnsStockItem));
        using var context = new InventoryDbContext(options);
        var itemId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        context.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = Guid.NewGuid(),
            Sku = "SKU-123",
            QuantityAvailable = 200,
            QuantityReserved = 20
        });
        await context.SaveChangesAsync();

        var handler = new GetStockItemByIdQueryHandler(context);
        var result = await handler.Handle(new GetStockItemByIdQuery(itemId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
        Assert.Equal("SKU-123", result.Sku);
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ReturnsNull()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithNonExistingId_ReturnsNull));
        using var context = new InventoryDbContext(options);

        var handler = new GetStockItemByIdQueryHandler(context);
        var result = await handler.Handle(new GetStockItemByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
