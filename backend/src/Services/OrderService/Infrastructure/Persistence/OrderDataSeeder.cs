using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Seeds the OrderService database with 2000 sample orders.
/// </summary>
public static class OrderDataSeeder
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

    private static readonly string[] SkuPrefixes = ["WM", "MK", "UC", "WC", "NH", "SW", "PC", "LS", "DL", "MN", "GC", "SD", "BS", "TS", "PC"];

    private static readonly string[] Cities =
    [
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio",
        "San Diego", "Dallas", "San Jose", "Austin", "Jacksonville", "Fort Worth", "Columbus",
        "Charlotte", "San Francisco", "Indianapolis", "Seattle", "Denver", "Washington",
        "Boston", "El Paso", "Nashville", "Detroit", "Oklahoma City", "Portland", "Las Vegas",
        "Louisville", "Baltimore", "Milwaukee", "Albuquerque", "Tucson", "Fresno", "Sacramento",
        "Mesa", "Kansas City", "Atlanta", "Long Beach", "Colorado Springs", "Raleigh"
    ];

    private static readonly string[] States =
    [
        "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA","KS","KY","LA",
        "ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY","NC","ND","OH","OK",
        "OR","PA","RI","SC","SD","TN","TX","UT","VT","VA","WA","WV","WI","WY"
    ];

    private static readonly string[] StreetNames =
    [
        "Main St", "Oak Ave", "Maple Dr", "Cedar Ln", "Pine Rd", "Elm St", "Washington Ave",
        "Lakeview Dr", "Riverside Rd", "Highland Ave", "Sunset Blvd", "Forest Ln", "Broadway",
        "Park Ave", "Center St", "Mill Rd", "Church St", "King Ave", "Hilltop Dr", "Valley Rd"
    ];

    /// <summary>
    /// Seeds the database with 2000 sample orders, items, and status histories.
    /// </summary>
    public static async Task SeedAsync(OrderDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Orders.AnyAsync())
        {
            logger.LogInformation("Database already contains orders. Skipping seed.");
            return;
        }

        var rng = new Random(42);
        const int orderCount = 2000;

        var orders = new List<Order>();
        var items = new List<OrderItem>();
        var histories = new List<OrderStatusHistory>();

        for (int i = 0; i < orderCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var customerId = Guid.NewGuid();
            var orderNumber = $"ORD-{i + 1:D7}";
            var createdAt = DateTime.UtcNow.AddDays(-rng.Next(0, 730)).AddHours(-rng.Next(0, 24));

            // Determine status based on age (older orders more likely completed)
            var ageDays = (DateTime.UtcNow - createdAt).TotalDays;
            var status = PickWeightedStatus(rng, ageDays);

            // Build items
            var itemCount = rng.Next(1, 6);
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            for (int j = 0; j < itemCount; j++)
            {
                var productName = ProductNames[rng.Next(ProductNames.Length)];
                var sku = $"{SkuPrefixes[rng.Next(SkuPrefixes.Length)]}-{rng.Next(1000, 9999)}";
                var unitPrice = Math.Round((decimal)(rng.NextDouble() * 290 + 10), 2); // $10 - $300
                var quantity = rng.Next(1, 5);
                var lineTotal = Math.Round(unitPrice * quantity, 2);
                subtotal += lineTotal;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductName = productName,
                    Sku = sku,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    LineTotal = lineTotal
                });
            }

            var shippingCost = Math.Round((decimal)(rng.NextDouble() * 20 + 5), 2);
            var taxAmount = Math.Round(subtotal * 0.08m, 2);
            var total = Math.Round(subtotal + shippingCost + taxAmount, 2);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerId,
                OrderNumber = orderNumber,
                Status = status,
                Subtotal = subtotal,
                ShippingCost = shippingCost,
                TaxAmount = taxAmount,
                Total = total,
                Currency = "USD",
                ShippingAddress = GenerateShippingAddress(rng),
                CreatedAt = createdAt,
                Items = orderItems
            };

            // Link items to order
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
            }

            orders.Add(order);
            items.AddRange(orderItems);

            // Status history
            var statusHistory = GenerateStatusHistory(order.Id, status, createdAt, rng);
            histories.AddRange(statusHistory);
        }

        await dbContext.Orders.AddRangeAsync(orders);
        await dbContext.OrderItems.AddRangeAsync(items);
        await dbContext.OrderStatusHistories.AddRangeAsync(histories);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded OrderService with {OrderCount} orders, {ItemCount} items, and {HistoryCount} status histories.",
            orders.Count, items.Count, histories.Count);
    }

    private static OrderStatus PickWeightedStatus(Random rng, double ageDays)
    {
        // Older orders more likely to be delivered/cancelled; newer more likely pending/paid
        var roll = rng.NextDouble();
        if (ageDays < 7)
        {
            if (roll < 0.30) return OrderStatus.Pending;
            if (roll < 0.60) return OrderStatus.InventoryReserved;
            if (roll < 0.85) return OrderStatus.PaymentInitiated;
            return OrderStatus.Paid;
        }
        if (ageDays < 30)
        {
            if (roll < 0.10) return OrderStatus.Pending;
            if (roll < 0.25) return OrderStatus.PaymentInitiated;
            if (roll < 0.70) return OrderStatus.Paid;
            if (roll < 0.90) return OrderStatus.Shipped;
            return OrderStatus.Delivered;
        }
        if (ageDays < 90)
        {
            if (roll < 0.05) return OrderStatus.Pending;
            if (roll < 0.15) return OrderStatus.Paid;
            if (roll < 0.50) return OrderStatus.Shipped;
            if (roll < 0.85) return OrderStatus.Delivered;
            if (roll < 0.95) return OrderStatus.Cancelled;
            return OrderStatus.Refunded;
        }
        // Very old
        if (roll < 0.05) return OrderStatus.Paid;
        if (roll < 0.60) return OrderStatus.Delivered;
        if (roll < 0.85) return OrderStatus.Cancelled;
        if (roll < 0.95) return OrderStatus.Refunded;
        return OrderStatus.Shipped;
    }

    private static List<OrderStatusHistory> GenerateStatusHistory(Guid orderId, OrderStatus finalStatus, DateTime orderCreatedAt, Random rng)
    {
        var history = new List<OrderStatusHistory>();
        var progression = new List<OrderStatus> { OrderStatus.Pending };

        switch (finalStatus)
        {
            case OrderStatus.InventoryReserved:
                progression.Add(OrderStatus.InventoryReserved);
                break;
            case OrderStatus.PaymentInitiated:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.PaymentInitiated);
                break;
            case OrderStatus.Paid:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.PaymentInitiated);
                progression.Add(OrderStatus.Paid);
                break;
            case OrderStatus.Shipped:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.PaymentInitiated);
                progression.Add(OrderStatus.Paid);
                progression.Add(OrderStatus.Shipped);
                break;
            case OrderStatus.Delivered:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.PaymentInitiated);
                progression.Add(OrderStatus.Paid);
                progression.Add(OrderStatus.Shipped);
                progression.Add(OrderStatus.Delivered);
                break;
            case OrderStatus.Cancelled:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.Cancelled);
                break;
            case OrderStatus.Refunded:
                progression.Add(OrderStatus.InventoryReserved);
                progression.Add(OrderStatus.PaymentInitiated);
                progression.Add(OrderStatus.Paid);
                progression.Add(OrderStatus.Shipped);
                progression.Add(OrderStatus.Delivered);
                progression.Add(OrderStatus.Refunded);
                break;
        }

        var currentTime = orderCreatedAt;
        for (int i = 0; i < progression.Count; i++)
        {
            var timeOffset = i == 0 ? TimeSpan.Zero : TimeSpan.FromMinutes(rng.Next(5, 1440));
            currentTime = currentTime.Add(timeOffset);

            history.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Status = progression[i],
                ChangedAt = currentTime,
                Reason = i == 0 ? "Order created" : $"Transitioned to {progression[i]}"
            });
        }

        return history;
    }

    private static string GenerateShippingAddress(Random rng)
    {
        var streetNumber = rng.Next(100, 9999);
        var street = StreetNames[rng.Next(StreetNames.Length)];
        var city = Cities[rng.Next(Cities.Length)];
        var state = States[rng.Next(States.Length)];
        var postalCode = rng.Next(10000, 99999).ToString("D5");

        var address = new
        {
            Street = $"{streetNumber} {street}",
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = "US"
        };

        return JsonSerializer.Serialize(address);
    }
}
