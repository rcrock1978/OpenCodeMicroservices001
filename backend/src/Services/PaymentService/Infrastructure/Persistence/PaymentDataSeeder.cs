using PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Persistence;

/// <summary>
/// Seeds the PaymentService database with 3000 sample payment records.
/// </summary>
public static class PaymentDataSeeder
{
    private static readonly Guid[] TenantIds =
    [
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444")
    ];

    private static readonly string[] CardBrands = ["Visa", "Mastercard", "Amex", "Discover"];
    private static readonly string[] FailureReasons = ["insufficient_funds", "card_declined", "expired_card", "incorrect_cvc", "processing_error", "bank_account_closed"];
    private static readonly string[] Currencies = ["USD", "EUR", "GBP", "CAD"];

    /// <summary>
    /// Seeds the database with 3000 payment records (PaymentIntents, PaymentMethods, PaymentTransactions).
    /// </summary>
    public static async Task SeedAsync(PaymentDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.PaymentIntents.AnyAsync())
        {
            logger.LogInformation("Database already contains payment data. Skipping seed.");
            return;
        }

        var rng = new Random(42);

        // Distribution: 1200 PaymentIntents + 800 PaymentMethods + 1000 PaymentTransactions = 3000 total
        const int intentCount = 1200;
        const int methodCount = 800;
        const int transactionCount = 1000;

        var intents = new List<PaymentIntent>();
        var methods = new List<PaymentMethod>();
        var transactions = new List<PaymentTransaction>();

        // Generate PaymentMethods first (they don't depend on other entities)
        for (int i = 0; i < methodCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var type = PickPaymentMethodType(rng);
            var brand = type == PaymentMethodType.Card ? CardBrands[rng.Next(CardBrands.Length)] : null;
            var lastFour = type == PaymentMethodType.Card ? rng.Next(1000, 9999).ToString() : null;
            var expMonth = type == PaymentMethodType.Card ? rng.Next(1, 13) : (int?)null;
            var expYear = type == PaymentMethodType.Card ? rng.Next(2025, 2031) : (int?)null;

            methods.Add(new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = Guid.NewGuid(),
                Type = type,
                LastFour = lastFour,
                Brand = brand,
                ExpMonth = expMonth,
                ExpYear = expYear,
                IsDefault = rng.NextDouble() < 0.3,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 730)).AddHours(-rng.Next(0, 24))
            });
        }

        await dbContext.PaymentMethods.AddRangeAsync(methods);
        await dbContext.SaveChangesAsync();

        // Generate PaymentIntents
        for (int i = 0; i < intentCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var amount = Math.Round((decimal)(rng.NextDouble() * 490 + 10), 2); // $10 - $500
            var currency = Currencies[rng.Next(Currencies.Length)];
            var status = PickPaymentStatus(rng);
            var createdAt = DateTime.UtcNow.AddDays(-rng.Next(0, 730)).AddHours(-rng.Next(0, 24));
            DateTime? capturedAt = null;

            if (status is PaymentStatus.Succeeded or PaymentStatus.Refunded)
            {
                capturedAt = createdAt.AddMinutes(rng.Next(1, 1440));
            }

            var paymentMethodSnapshot = rng.NextDouble() < 0.8
                ? $"{{\"type\":\"card\",\"brand\":\"{CardBrands[rng.Next(CardBrands.Length)]}\",\"last4\":\"{rng.Next(1000, 9999)}\"}}"
                : $"{{\"type\":\"wallet\",\"provider\":\"PayPal\"}}";

            intents.Add(new PaymentIntent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Amount = amount,
                Currency = currency,
                IdempotencyKey = $"idem_{Guid.NewGuid():N}_{i:D4}",
                Status = status,
                PaymentMethod = paymentMethodSnapshot,
                FailureReason = status == PaymentStatus.Failed ? FailureReasons[rng.Next(FailureReasons.Length)] : null,
                CreatedAt = createdAt,
                CapturedAt = capturedAt
            });
        }

        await dbContext.PaymentIntents.AddRangeAsync(intents);
        await dbContext.SaveChangesAsync();

        // Generate PaymentTransactions referencing existing PaymentIntents
        for (int i = 0; i < transactionCount; i++)
        {
            var intent = intents[rng.Next(intents.Count)];
            var type = PickTransactionType(rng);
            var status = PickTransactionStatus(rng);
            var amount = type == PaymentTransactionType.Refund
                ? Math.Round(intent.Amount * (decimal)rng.NextDouble(), 2)
                : intent.Amount;

            var gatewayResponse = status == PaymentTransactionStatus.Succeeded
                ? $"{{\"id\":\"txn_{Guid.NewGuid():N}\",\"status\":\"succeeded\"}}"
                : $"{{\"id\":\"txn_{Guid.NewGuid():N}\",\"status\":\"failed\",\"error\":\"{FailureReasons[rng.Next(FailureReasons.Length)]}\"}}";

            transactions.Add(new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PaymentIntentId = intent.Id,
                Type = type,
                Amount = amount,
                Status = status,
                GatewayResponse = gatewayResponse,
                CreatedAt = intent.CreatedAt.AddMinutes(rng.Next(1, 1440))
            });
        }

        await dbContext.PaymentTransactions.AddRangeAsync(transactions);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded PaymentService with {IntentCount} payment intents, {MethodCount} payment methods, and {TransactionCount} transactions.",
            intents.Count, methods.Count, transactions.Count);
    }

    private static PaymentMethodType PickPaymentMethodType(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.70) return PaymentMethodType.Card;
        if (roll < 0.90) return PaymentMethodType.Wallet;
        return PaymentMethodType.BankTransfer;
    }

    private static PaymentStatus PickPaymentStatus(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.15) return PaymentStatus.Pending;
        if (roll < 0.25) return PaymentStatus.Processing;
        if (roll < 0.65) return PaymentStatus.Succeeded;
        if (roll < 0.85) return PaymentStatus.Failed;
        if (roll < 0.95) return PaymentStatus.Cancelled;
        return PaymentStatus.Refunded;
    }

    private static PaymentTransactionType PickTransactionType(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.45) return PaymentTransactionType.Authorization;
        if (roll < 0.80) return PaymentTransactionType.Capture;
        if (roll < 0.95) return PaymentTransactionType.Refund;
        return PaymentTransactionType.Void;
    }

    private static PaymentTransactionStatus PickTransactionStatus(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.75) return PaymentTransactionStatus.Succeeded;
        if (roll < 0.90) return PaymentTransactionStatus.Failed;
        return PaymentTransactionStatus.Pending;
    }
}
