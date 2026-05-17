using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

/// <summary>
/// Seeds the InventoryService database with 3000 sample inventory records.
/// </summary>
public static class InventoryDataSeeder
{
    private static readonly Guid[] TenantIds =
    [
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444")
    ];

    private static readonly string[] ProductNames =
    [
        "Wireless Mouse", "Mechanical Keyboard", "USB-C Hub", "Webcam 4K", "Noise Cancelling Headphones",
        "Smart Watch", "Portable Charger", "Laptop Stand", "Desk Lamp LED", "Monitor 27\"",
        "Gaming Chair", "Standing Desk", "Bluetooth Speaker", "Tablet Stand", "Phone Case",
        "Screen Protector", "HDMI Cable", "Router WiFi 6", "SSD 1TB", "RAM 32GB",
        "Graphics Card", "CPU Cooler", "Power Supply", "PC Case", "Motherboard",
        "Action Camera", "Drone Mini", "Smart Thermostat", "Robot Vacuum", "Air Purifier",
        "Coffee Maker", "Electric Kettle", "Blender Pro", "Rice Cooker", "Microwave Oven",
        "Toaster Oven", "Food Processor", "Slow Cooker", "Pressure Cooker", "Juicer",
        "Running Shoes", "Yoga Mat", "Resistance Bands", "Dumbbell Set", "Treadmill",
        "Exercise Bike", "Foam Roller", "Gym Bag", "Water Bottle", "Fitness Tracker",
        "Winter Jacket", "Denim Jeans", "Cotton T-Shirt", "Wool Sweater", "Leather Belt",
        "Sneakers", "Baseball Cap", "Sunglasses", "Backpack", "Wallet",
        "Board Game", "Puzzle 1000pc", "Chess Set", "Playing Cards", "Model Kit",
        "Art Supplies", "Guitar Strings", "Piano Keyboard", "Microphone", "Drawing Tablet",
        "Garden Tools Set", "Plant Pot", "LED Grow Light", "Hose Reel", "Compost Bin",
        "Pet Bed", "Dog Leash", "Cat Tower", "Fish Tank", "Bird Feeder",
        "Car Phone Mount", "Dash Cam", "Tire Inflator", "Seat Covers", "Floor Mats",
        "Baby Monitor", "Stroller", "Diaper Bag", "Bottle Warmer", "Baby Carrier"
    ];

    private static readonly string[] SkuPrefixes = ["WM", "MK", "UC", "WC", "NH", "SW", "PC", "LS", "DL", "MN", "GC", "SD", "BS", "TS", "PC", "SP", "HC", "RW", "SD", "RM", "GC", "CC", "PS", "PC", "MB", "AC", "DM", "ST", "RV", "AP", "CM", "EK", "BP", "RC", "MO", "TO", "FP", "SC", "PC", "JU", "RS", "YM", "RB", "DS", "TM", "EB", "FR", "GB", "WB", "FT", "WJ", "DJ", "CT", "WS", "LB", "SN", "BC", "SG", "BP", "WL", "BG", "PZ", "CS", "PD", "MK", "AS", "GS", "PK", "MC", "DT", "GT", "PP", "LG", "HR", "CB", "PB", "DL", "CT", "FT", "BF", "CP", "DC", "TI", "SC", "FM", "BM", "ST", "DB", "BW", "BC"];

    /// <summary>
    /// Seeds the database with 3000 inventory records (StockItems, StockMovements, StockReservations).
    /// </summary>
    public static async Task SeedAsync(InventoryDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.StockItems.AnyAsync())
        {
            logger.LogInformation("Database already contains inventory data. Skipping seed.");
            return;
        }

        var rng = new Random(42);

        // Distribution: 1200 StockItems + 1200 StockMovements + 600 StockReservations = 3000 total
        const int stockItemCount = 1200;
        const int movementCount = 1200;
        const int reservationCount = 600;

        var stockItems = new List<StockItem>();
        var movements = new List<StockMovement>();
        var reservations = new List<StockReservation>();

        // Generate StockItems
        for (int i = 0; i < stockItemCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var productName = ProductNames[rng.Next(ProductNames.Length)];
            var sku = $"{SkuPrefixes[rng.Next(SkuPrefixes.Length)]}-{rng.Next(1000, 9999)}-{i:D4}";
            var available = rng.Next(0, 501);
            var reserved = rng.Next(0, Math.Min(available + 1, 51));
            var lowThreshold = rng.Next(5, 21);

            stockItems.Add(new StockItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductVariantId = Guid.NewGuid(),
                Sku = sku,
                QuantityAvailable = available - reserved,
                QuantityReserved = reserved,
                LowStockThreshold = lowThreshold,
                UpdatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 90)).AddHours(-rng.Next(0, 24))
            });
        }

        await dbContext.StockItems.AddRangeAsync(stockItems);
        await dbContext.SaveChangesAsync();

        // Generate StockMovements referencing existing StockItems
        for (int i = 0; i < movementCount; i++)
        {
            var stockItem = stockItems[rng.Next(stockItems.Count)];
            var type = PickMovementType(rng);
            var quantity = rng.Next(1, 101);
            var reference = type switch
            {
                StockMovementType.Inbound => $"PO-{rng.Next(1000, 9999)}",
                StockMovementType.Outbound => $"SO-{rng.Next(1000, 9999)}",
                StockMovementType.Adjustment => $"ADJ-{rng.Next(1000, 9999)}",
                StockMovementType.Reservation => $"RES-{rng.Next(1000, 9999)}",
                StockMovementType.Release => $"REL-{rng.Next(1000, 9999)}",
                _ => $"REF-{rng.Next(1000, 9999)}"
            };

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = stockItem.TenantId,
                StockItemId = stockItem.Id,
                Type = type,
                Quantity = quantity,
                Reference = reference,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 365)).AddHours(-rng.Next(0, 24))
            });
        }

        // Generate StockReservations referencing existing StockItems
        for (int i = 0; i < reservationCount; i++)
        {
            var stockItem = stockItems[rng.Next(stockItems.Count)];
            var quantity = rng.Next(1, Math.Min(stockItem.QuantityAvailable + stockItem.QuantityReserved + 1, 21));
            var status = PickReservationStatus(rng);
            var createdAt = DateTime.UtcNow.AddDays(-rng.Next(0, 30)).AddHours(-rng.Next(0, 24));
            var expiresAt = createdAt.AddMinutes(rng.Next(15, 2881)); // 15 min to 48 hours

            reservations.Add(new StockReservation
            {
                Id = Guid.NewGuid(),
                TenantId = stockItem.TenantId,
                OrderId = Guid.NewGuid(),
                StockItemId = stockItem.Id,
                Quantity = quantity,
                Status = status,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            });
        }

        await dbContext.StockMovements.AddRangeAsync(movements);
        await dbContext.StockReservations.AddRangeAsync(reservations);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded InventoryService with {StockItemCount} stock items, {MovementCount} movements, and {ReservationCount} reservations.",
            stockItems.Count, movements.Count, reservations.Count);
    }

    private static StockMovementType PickMovementType(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.35) return StockMovementType.Inbound;
        if (roll < 0.60) return StockMovementType.Outbound;
        if (roll < 0.80) return StockMovementType.Adjustment;
        if (roll < 0.90) return StockMovementType.Reservation;
        return StockMovementType.Release;
    }

    private static ReservationStatus PickReservationStatus(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.40) return ReservationStatus.Reserved;
        if (roll < 0.70) return ReservationStatus.Committed;
        if (roll < 0.85) return ReservationStatus.Released;
        return ReservationStatus.Expired;
    }
}
