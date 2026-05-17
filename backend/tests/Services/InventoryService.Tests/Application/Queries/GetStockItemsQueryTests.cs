using InventoryService.Application.Queries;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Application.Queries;

/// <summary>
/// Unit tests for the <see cref="GetStockItemsQueryHandler"/>.
/// </summary>
public class GetStockItemsQueryTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task Handle_WithSeededItems_ReturnsAllStockItems()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithSeededItems_ReturnsAllStockItems));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();

        context.StockItems.AddRange(
            new StockItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductVariantId = Guid.NewGuid(),
                Sku = "SKU-001",
                QuantityAvailable = 100,
                QuantityReserved = 0
            },
            new StockItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductVariantId = Guid.NewGuid(),
                Sku = "SKU-002",
                QuantityAvailable = 50,
                QuantityReserved = 5
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetStockItemsQueryHandler(context);
        var result = await handler.Handle(new GetStockItemsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Sku == "SKU-001");
        Assert.Contains(result, s => s.Sku == "SKU-002");
    }

    [Fact]
    public async Task Handle_WithEmptyDb_ReturnsEmptyList()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithEmptyDb_ReturnsEmptyList));
        using var context = new InventoryDbContext(options);

        var handler = new GetStockItemsQueryHandler(context);
        var result = await handler.Handle(new GetStockItemsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
