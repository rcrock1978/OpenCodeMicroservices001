using InventoryService.Application.Commands;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Application.Commands;

/// <summary>
/// Unit tests for the <see cref="CreateStockItemCommandHandler"/>.
/// </summary>
public class CreateStockItemCommandTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesStockItem()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithValidCommand_CreatesStockItem));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        var handler = new CreateStockItemCommandHandler(context);
        var command = new CreateStockItemCommand(
            TenantId: tenantId,
            ProductVariantId: productVariantId,
            Sku: "NEW-SKU",
            QuantityAvailable: 75,
            LowStockThreshold: 5);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(productVariantId, result.ProductVariantId);
        Assert.Equal("NEW-SKU", result.Sku);
        Assert.Equal(75, result.QuantityAvailable);
        Assert.Equal(0, result.QuantityReserved);
        Assert.Equal(5, result.LowStockThreshold);

        var persisted = await context.StockItems.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal("NEW-SKU", persisted.Sku);
    }
}
